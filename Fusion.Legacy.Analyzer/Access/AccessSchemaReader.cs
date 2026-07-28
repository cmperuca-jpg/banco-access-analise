using System.Data;
using System.Data.OleDb;

namespace Fusion.Legacy.Analyzer.Access;

public static class AccessSchemaReader
{
    public static List<string> GetTableNames(OleDbConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        if (connection.State != ConnectionState.Open)
            connection.Open();

        var tableNames = new List<string>();

        using var schema = connection.GetOleDbSchemaTable(
            OleDbSchemaGuid.Tables,
            new object?[] { null, null, null, "TABLE" }
        );

        if (schema is null)
            return tableNames;

        foreach (DataRow row in schema.Rows)
        {
            var tableName = row["TABLE_NAME"]?.ToString();

            if (!string.IsNullOrWhiteSpace(tableName))
                tableNames.Add(tableName);
        }

        tableNames.Sort(StringComparer.OrdinalIgnoreCase);

        return tableNames;
    }
}