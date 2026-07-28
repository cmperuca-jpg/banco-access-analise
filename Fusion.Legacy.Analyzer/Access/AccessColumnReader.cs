using System.Data;
using System.Data.OleDb;

namespace Fusion.Legacy.Analyzer.Access;

public static class AccessColumnReader
{
    public static List<AccessColumnInfo> GetColumns(
        OleDbConnection connection,
        string tableName)
    {
        ArgumentNullException.ThrowIfNull(connection);

        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("O nome da tabela não foi informado.");

        if (connection.State != ConnectionState.Open)
            connection.Open();

        var autoIncrementColumns = GetAutoIncrementColumns(connection, tableName);
        var columns = new List<AccessColumnInfo>();

        using var schema = connection.GetOleDbSchemaTable(
            OleDbSchemaGuid.Columns,
            new object?[] { null, null, tableName, null }
        );

        if (schema is null)
            return columns;

        foreach (DataRow row in schema.Rows)
        {
            var dataType = GetInt(row, "DATA_TYPE");
            var columnName = GetString(row, "COLUMN_NAME");

            columns.Add(new AccessColumnInfo
            {
                TableName = tableName,
                ColumnName = columnName,
                OrdinalPosition = GetInt(row, "ORDINAL_POSITION"),
                DataType = dataType,
                DataTypeName = GetOleDbTypeName(dataType),
                CharacterMaximumLength = GetNullableInt(row, "CHARACTER_MAXIMUM_LENGTH"),
                NumericPrecision = GetNullableInt(row, "NUMERIC_PRECISION"),
                NumericScale = GetNullableInt(row, "NUMERIC_SCALE"),
                IsNullable = GetBool(row, "IS_NULLABLE"),
                IsAutoIncrement = autoIncrementColumns.Contains(columnName)
            });
        }

        return columns
            .OrderBy(column => column.OrdinalPosition)
            .ToList();
    }

    private static HashSet<string> GetAutoIncrementColumns(
        OleDbConnection connection,
        string tableName)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var quotedTableName = "[" + tableName.Replace("]", "]]", StringComparison.Ordinal) + "]";

        using var command = new OleDbCommand(
            $"SELECT * FROM {quotedTableName} WHERE 1 = 0",
            connection
        );

        using var reader = command.ExecuteReader(CommandBehavior.SchemaOnly);
        var schema = reader?.GetSchemaTable();

        if (schema is null)
            return result;

        foreach (DataRow row in schema.Rows)
        {
            if (!GetBool(row, "IsAutoIncrement"))
                continue;

            var columnName = GetString(row, "BaseColumnName");

            if (string.IsNullOrWhiteSpace(columnName))
                columnName = GetString(row, "ColumnName");

            if (!string.IsNullOrWhiteSpace(columnName))
                result.Add(columnName);
        }

        return result;
    }

    private static string GetString(DataRow row, string columnName)
    {
        return row.Table.Columns.Contains(columnName)
            ? row[columnName]?.ToString() ?? string.Empty
            : string.Empty;
    }

    private static int GetInt(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return 0;

        var value = row[columnName];
        return value is DBNull ? 0 : Convert.ToInt32(value);
    }

    private static int? GetNullableInt(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return null;

        var value = row[columnName];
        return value is DBNull ? null : Convert.ToInt32(value);
    }

    private static bool GetBool(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return false;

        var value = row[columnName];
        return value is not DBNull && Convert.ToBoolean(value);
    }

    private static string GetOleDbTypeName(int dataType)
    {
        return Enum.IsDefined(typeof(OleDbType), dataType)
            ? ((OleDbType)dataType).ToString()
            : $"Desconhecido ({dataType})";
    }
}
