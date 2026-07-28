using System.Data.OleDb;
using Fusion.Legacy.Analyzer.Access;
using Fusion.Legacy.Analyzer.Model;

namespace Fusion.Legacy.Analyzer.Builders;

public static class AccessDatabaseModelBuilder
{
    public static DatabaseModel Build(OleDbConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        var database = new DatabaseModel();

        var tableNames = AccessSchemaReader
            .GetTableNames(connection)
            .Where(tableName =>
                !tableName.StartsWith(
                    "MSys",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            .OrderBy(tableName => tableName)
            .ToList();

        var relations = AccessRelationReader
            .GetRelations(connection)
            .Where(relation =>
                !relation.PrimaryTable.StartsWith(
                    "MSys",
                    StringComparison.OrdinalIgnoreCase
                )
                &&
                !relation.ForeignTable.StartsWith(
                    "MSys",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            .ToList();

        var relationRules = AccessRelationRuleReader
            .GetRelationRules(connection)
            .Where(rule =>
                !rule.RelationName.StartsWith(
                    "MSys",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            .ToDictionary(
                rule => rule.RelationName,
                rule => rule,
                StringComparer.OrdinalIgnoreCase
            );

        foreach (var tableName in tableNames)
        {
            var table = new TableModel
            {
                Name = tableName
            };

            AddColumns(connection, table);
            AddIndexes(connection, table);
            AddForeignKeys(table, relations, relationRules);

            database.Tables.Add(table);
        }

        return database;
    }

    private static void AddColumns(
        OleDbConnection connection,
        TableModel table)
    {
        var columns = AccessColumnReader
            .GetColumns(connection, table.Name)
            .OrderBy(column => column.OrdinalPosition);

        foreach (var column in columns)
        {
            table.Columns.Add(new ColumnModel
            {
                Name = column.ColumnName,
                AccessType = column.DataType,
                AccessTypeName = column.DataTypeName,
                CharacterMaximumLength = column.CharacterMaximumLength,
                NumericPrecision = column.NumericPrecision,
                NumericScale = column.NumericScale,
                IsNullable = column.IsNullable,
                IsAutoIncrement = column.IsAutoIncrement,
                OrdinalPosition = column.OrdinalPosition
            });
        }
    }

    private static void AddIndexes(
        OleDbConnection connection,
        TableModel table)
    {
        var accessIndexes = AccessIndexReader.GetIndexes(
            connection,
            table.Name
        );

        var indexGroups = accessIndexes
            .GroupBy(
                index => index.IndexName,
                StringComparer.OrdinalIgnoreCase
            )
            .OrderBy(group => group.Key);

        foreach (var indexGroup in indexGroups)
        {
            var firstIndex = indexGroup.First();

            var index = new IndexModel
            {
                Name = firstIndex.IndexName,
                IsPrimaryKey = firstIndex.IsPrimaryKey,
                IsUnique = firstIndex.IsUnique
            };

            foreach (var indexColumn in indexGroup
                         .OrderBy(item => item.OrdinalPosition))
            {
                index.Columns.Add(new IndexColumnModel
                {
                    Name = indexColumn.ColumnName,
                    OrdinalPosition = indexColumn.OrdinalPosition
                });
            }

            table.Indexes.Add(index);
        }
    }

    private static void AddForeignKeys(
        TableModel table,
        IReadOnlyCollection<AccessRelationInfo> relations,
        IReadOnlyDictionary<string, AccessRelationRule> rules)
    {
        var tableRelations = relations
            .Where(relation =>
                string.Equals(
                    relation.ForeignTable,
                    table.Name,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            .GroupBy(
                relation => relation.RelationName,
                StringComparer.OrdinalIgnoreCase
            )
            .OrderBy(group => group.Key);

        foreach (var relationGroup in tableRelations)
        {
            var firstRelation = relationGroup.First();

            rules.TryGetValue(
                firstRelation.RelationName,
                out var relationRule
            );

            var foreignKey = new ForeignKeyModel
            {
                Name = firstRelation.RelationName,
                ReferencedTable = firstRelation.PrimaryTable,
                CascadeUpdate = relationRule?.CascadeUpdate ?? false,
                CascadeDelete = relationRule?.CascadeDelete ?? false
            };

            foreach (var relation in relationGroup
                         .OrderBy(item => item.OrdinalPosition))
            {
                foreignKey.Columns.Add(
                    new ForeignKeyColumnModel
                    {
                        ColumnName = relation.ForeignColumn,
                        ReferencedColumnName = relation.PrimaryColumn,
                        OrdinalPosition = relation.OrdinalPosition
                    }
                );
            }

            table.ForeignKeys.Add(foreignKey);
        }
    }
}