using System.Data;
using System.Data.OleDb;

namespace Fusion.Legacy.Analyzer.Access;

public static class AccessRelationRuleReader
{
    public static List<AccessRelationRule> GetRelationRules(
        OleDbConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        if (connection.State != ConnectionState.Open)
            connection.Open();

        var rules = new List<AccessRelationRule>();

        using var schema = connection.GetOleDbSchemaTable(
            OleDbSchemaGuid.Foreign_Keys,
            null
        );

        if (schema is null)
            return rules;

        foreach (DataRow row in schema.Rows)
        {
            var relationName = GetString(row, "FK_NAME");

            var updateRule = GetRuleValue(
                row,
                "UPDATE_RULE"
            );

            var deleteRule = GetRuleValue(
                row,
                "DELETE_RULE"
            );

            rules.Add(new AccessRelationRule
            {
                RelationName = relationName,
                CascadeUpdate = IsCascade(updateRule),
                CascadeDelete = IsCascade(deleteRule),

                // Essas propriedades não representam regras
                // referenciais disponíveis no catálogo Foreign_Keys.
                LeftTable = false,
                RightTable = false
            });
        }

        return rules
            .GroupBy(rule => rule.RelationName)
            .Select(group => group.First())
            .OrderBy(rule => rule.RelationName)
            .ToList();
    }

    private static object? GetRuleValue(
        DataRow row,
        string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return null;

        var value = row[columnName];

        return value is DBNull
            ? null
            : value;
    }

    private static bool IsCascade(object? value)
    {
        if (value is null)
            return false;

        var text = value
            .ToString()?
            .Trim();

        if (string.Equals(
            text,
            "CASCADE",
            StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        /*
         * Alguns provedores retornam a regra CASCADE
         * como valor numérico zero no catálogo
         * de chaves estrangeiras.
         */
        return int.TryParse(text, out var numericValue)
            && numericValue == 0;
    }

    private static string GetString(
        DataRow row,
        string columnName)
    {
        if (!row.Table.Columns.Contains(columnName))
            return string.Empty;

        var value = row[columnName];

        return value is DBNull
            ? string.Empty
            : value.ToString() ?? string.Empty;
    }
}