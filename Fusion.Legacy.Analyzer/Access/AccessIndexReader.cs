using System.Data;
using System.Data.OleDb;

namespace Fusion.Legacy.Analyzer.Access;

public static class AccessIndexReader
{
    public static List<AccessIndexInfo> GetIndexes(
        OleDbConnection connection,
        string tableName)
    {
        ArgumentNullException.ThrowIfNull(connection);

        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("O nome da tabela não foi informado.");

        if (connection.State != ConnectionState.Open)
            connection.Open();

        var indexes = new List<AccessIndexInfo>();

        using var schema = connection.GetOleDbSchemaTable(
            OleDbSchemaGuid.Indexes,
            new object?[]
            {
                null,
                null,
                null,
                null,
                tableName
            });

        if (schema is null)
            return indexes;

        foreach (DataRow row in schema.Rows)
        {
            indexes.Add(new AccessIndexInfo
            {
                TableName = tableName,
                IndexName = GetString(row, "INDEX_NAME"),
                ColumnName = GetString(row, "COLUMN_NAME"),
                OrdinalPosition = GetInt(row, "ORDINAL_POSITION"),
                IsPrimaryKey = GetBool(row, "PRIMARY_KEY"),
                IsUnique = GetBool(row, "UNIQUE")
            });
        }

        return indexes
            .OrderBy(index => index.IndexName)
            .ThenBy(index => index.OrdinalPosition)
            .ToList();
    }

    private static string GetString(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return string.Empty;

        var value = row[columnName];

        return value is DBNull
            ? string.Empty
            : value.ToString() ?? string.Empty;
    }

    private static int GetInt(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return 0;

        var value = row[columnName];

        return value is DBNull
            ? 0
            : Convert.ToInt32(value);
    }

    private static bool GetBool(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return false;

        var value = row[columnName];

        return value is not DBNull && Convert.ToBoolean(value);
    }
}