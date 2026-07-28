using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Text;
using Fusion.Legacy.Analyzer.Model;

namespace Fusion.Legacy.Analyzer.PostgreSql;

public static class PostgresDataSqlGenerator
{
    public static void WriteDataFile(
        OleDbConnection connection,
        DatabaseModel database,
        string outputPath,
        Action<string>? progress = null)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(database);

        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("O caminho de saída não foi informado.", nameof(outputPath));

        if (connection.State != ConnectionState.Open)
            connection.Open();

        var fullPath = Path.GetFullPath(outputPath);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        using var writer = new StreamWriter(
            fullPath,
            false,
            new UTF8Encoding(false),
            bufferSize: 1024 * 1024);

        writer.WriteLine("-- Gerado por Fusion.Legacy.Analyzer");
        writer.WriteLine("-- Dados extraídos do Microsoft Access");
        writer.WriteLine("-- Execute este arquivo depois de postgresql-schema.sql");
        writer.WriteLine("BEGIN;");
        writer.WriteLine();

        foreach (var table in OrderTablesForInsert(database))
        {
            progress?.Invoke($"Exportando dados: {table.Name}");
            AppendTableData(writer, connection, table);
        }

        AppendIdentitySequenceAdjustments(writer, database);

        writer.WriteLine("COMMIT;");
    }

    private static void AppendTableData(
        TextWriter writer,
        OleDbConnection connection,
        TableModel table)
    {
        var columns = table.Columns
            .OrderBy(column => column.OrdinalPosition)
            .ToList();

        if (columns.Count == 0)
            return;

        writer.WriteLine($"-- Tabela: {table.Name}");

        var selectColumns = string.Join(", ", columns.Select(column => QuoteAccessIdentifier(column.Name)));
        var commandText = $"SELECT {selectColumns} FROM {QuoteAccessIdentifier(table.Name)}";

        using var command = new OleDbCommand(commandText, connection)
        {
            CommandTimeout = 0
        };

        using var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

        if (reader is null)
            throw new InvalidOperationException($"Não foi possível ler a tabela '{table.Name}'.");

        var postgresColumns = string.Join(", ", columns.Select(column => PostgresIdentifier.Quote(column.Name)));
        var hasIdentity = columns.Any(column => column.IsAutoIncrement);
        long rowCount = 0;

        while (reader.Read())
        {
            var values = new string[columns.Count];

            for (var index = 0; index < columns.Count; index++)
            {
                var value = reader.IsDBNull(index) ? null : reader.GetValue(index);
                values[index] = ToPostgresLiteral(value, columns[index]);
            }

            writer.Write("INSERT INTO ");
            writer.Write(PostgresIdentifier.Quote(table.Name));
            writer.Write(" (");
            writer.Write(postgresColumns);
            writer.Write(')');

            if (hasIdentity)
                writer.Write(" OVERRIDING SYSTEM VALUE");

            writer.Write(" VALUES (");
            writer.Write(string.Join(", ", values));
            writer.WriteLine(");");

            rowCount++;
        }

        writer.WriteLine($"-- Registros exportados: {rowCount}");
        writer.WriteLine();
    }

    private static string ToPostgresLiteral(object? value, ColumnModel column)
    {
        if (value is null || value is DBNull)
            return "NULL";

        return value switch
        {
            bool booleanValue => booleanValue ? "TRUE" : "FALSE",
            byte[] bytes => $"decode('{Convert.ToHexString(bytes)}', 'hex')",
            DateTime dateTime => QuoteString(dateTime.ToString("yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture)),
            DateTimeOffset dateTimeOffset => QuoteString(dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture)),
            Guid guid => QuoteString(guid.ToString("D")),
            string text => QuoteString(text),
            char character => QuoteString(character.ToString()),
            sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal
                => Convert.ToString(value, CultureInfo.InvariantCulture)
                   ?? throw new InvalidOperationException($"Valor numérico inválido na coluna '{column.Name}'."),
            _ => QuoteString(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty)
        };
    }

    private static string QuoteString(string value)
    {
        return "'" + value.Replace("'", "''") + "'";
    }

    private static string QuoteAccessIdentifier(string identifier)
    {
        return "[" + identifier.Replace("]", "]]", StringComparison.Ordinal) + "]";
    }

    private static IReadOnlyList<TableModel> OrderTablesForInsert(DatabaseModel database)
    {
        var tablesByName = database.Tables.ToDictionary(
            table => table.Name,
            StringComparer.OrdinalIgnoreCase);

        var dependencies = database.Tables.ToDictionary(
            table => table.Name,
            table => new HashSet<string>(
                table.ForeignKeys
                    .Select(foreignKey => foreignKey.ReferencedTable)
                    .Where(tablesByName.ContainsKey)
                    .Where(referencedTable => !referencedTable.Equals(table.Name, StringComparison.OrdinalIgnoreCase)),
                StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);

        var result = new List<TableModel>(database.Tables.Count);
        var remaining = new HashSet<string>(tablesByName.Keys, StringComparer.OrdinalIgnoreCase);

        while (remaining.Count > 0)
        {
            var ready = remaining
                .Where(tableName => dependencies[tableName].All(dependency => !remaining.Contains(dependency)))
                .OrderBy(tableName => tableName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (ready.Count == 0)
            {
                // Há ciclo entre relacionamentos. Mantém ordem determinística.
                ready.Add(remaining.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).First());
            }

            foreach (var tableName in ready)
            {
                result.Add(tablesByName[tableName]);
                remaining.Remove(tableName);
            }
        }

        return result;
    }

    private static void AppendIdentitySequenceAdjustments(
        TextWriter writer,
        DatabaseModel database)
    {
        var identityColumns = database.Tables
            .SelectMany(table => table.Columns
                .Where(column => column.IsAutoIncrement)
                .Select(column => (Table: table, Column: column)))
            .ToList();

        if (identityColumns.Count == 0)
            return;

        writer.WriteLine("-- Ajuste das sequências das colunas IDENTITY");

        foreach (var item in identityColumns)
        {
            var tableNameLiteral = QuoteString(PostgresIdentifier.Quote(item.Table.Name));
            var columnNameLiteral = QuoteString(item.Column.Name);
            var tableIdentifier = PostgresIdentifier.Quote(item.Table.Name);
            var columnIdentifier = PostgresIdentifier.Quote(item.Column.Name);

            writer.Write("SELECT setval(pg_get_serial_sequence(");
            writer.Write(tableNameLiteral);
            writer.Write(", ");
            writer.Write(columnNameLiteral);
            writer.Write("), COALESCE((SELECT MAX(");
            writer.Write(columnIdentifier);
            writer.Write(") FROM ");
            writer.Write(tableIdentifier);
            writer.Write("), 1), EXISTS (SELECT 1 FROM ");
            writer.Write(tableIdentifier);
            writer.WriteLine("));");
        }

        writer.WriteLine();
    }
}
