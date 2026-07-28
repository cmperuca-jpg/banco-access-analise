using System.Data;
using System.Data.OleDb;

namespace Fusion.Legacy.Analyzer.Access;

public static class AccessRelationReader
{
    public static List<AccessRelationInfo> GetRelations(
        OleDbConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        if (connection.State != ConnectionState.Open)
            connection.Open();

        var relations = new List<AccessRelationInfo>();

        using var schema = connection.GetOleDbSchemaTable(
            OleDbSchemaGuid.Foreign_Keys,
            null
        );

        if (schema is null)
            return relations;

        foreach (DataRow row in schema.Rows)
        {
            relations.Add(new AccessRelationInfo
            {
                RelationName = GetString(row, "FK_NAME"),
                PrimaryTable = GetString(row, "PK_TABLE_NAME"),
                PrimaryColumn = GetString(row, "PK_COLUMN_NAME"),
                ForeignTable = GetString(row, "FK_TABLE_NAME"),
                ForeignColumn = GetString(row, "FK_COLUMN_NAME"),
                OrdinalPosition = GetInt(row, "ORDINAL")
            });
        }

        return relations
            .OrderBy(relation => relation.RelationName)
            .ThenBy(relation => relation.OrdinalPosition)
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
}