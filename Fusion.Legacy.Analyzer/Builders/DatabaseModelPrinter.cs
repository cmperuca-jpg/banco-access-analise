using Fusion.Legacy.Analyzer.Model;

namespace Fusion.Legacy.Analyzer.Builders;

public static class DatabaseModelPrinter
{
    public static void Print(DatabaseModel database)
    {
        ArgumentNullException.ThrowIfNull(database);

        Console.WriteLine();
        Console.WriteLine(new string('=', 80));
        Console.WriteLine("MODELO INTERMEDIÁRIO DO BANCO");
        Console.WriteLine(new string('=', 80));

        Console.WriteLine(
            $"Total de tabelas da aplicação: {database.Tables.Count}"
        );

        Console.WriteLine();

        foreach (var table in database.Tables)
        {
            PrintTable(table);
        }
    }

    private static void PrintTable(TableModel table)
    {
        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"TABELA: {table.Name}");
        Console.WriteLine(new string('-', 80));

        Console.WriteLine($"Colunas: {table.Columns.Count}");

        foreach (var column in table.Columns
                     .OrderBy(item => item.OrdinalPosition))
        {
            Console.WriteLine(
                $"  {column.OrdinalPosition}: " +
                $"{column.Name} | " +
                $"Tipo={column.AccessTypeName} ({column.AccessType}) | " +
                $"Nulo={column.IsNullable} | " +
                $"AutoIncremento={column.IsAutoIncrement}"
            );
        }

        Console.WriteLine();
        Console.WriteLine($"Índices: {table.Indexes.Count}");

        if (table.Indexes.Count == 0)
        {
            Console.WriteLine("  Nenhum índice.");
        }
        else
        {
            foreach (var index in table.Indexes)
            {
                var columns = string.Join(
                    ", ",
                    index.Columns
                        .OrderBy(item => item.OrdinalPosition)
                        .Select(item => item.Name)
                );

                Console.WriteLine(
                    $"  {index.Name} | " +
                    $"Colunas=[{columns}] | " +
                    $"Primária={index.IsPrimaryKey} | " +
                    $"Único={index.IsUnique}"
                );
            }
        }

        Console.WriteLine();
        Console.WriteLine(
            $"Chaves estrangeiras: {table.ForeignKeys.Count}"
        );

        if (table.ForeignKeys.Count == 0)
        {
            Console.WriteLine("  Nenhuma chave estrangeira.");
        }
        else
        {
            foreach (var foreignKey in table.ForeignKeys)
            {
                var mappings = string.Join(
                    ", ",
                    foreignKey.Columns
                        .OrderBy(item => item.OrdinalPosition)
                        .Select(item =>
                            $"{item.ColumnName} -> " +
                            $"{foreignKey.ReferencedTable}." +
                            $"{item.ReferencedColumnName}"
                        )
                );

                Console.WriteLine(
                    $"  {foreignKey.Name} | " +
                    $"{mappings} | " +
                    $"CascadeUpdate={foreignKey.CascadeUpdate} | " +
                    $"CascadeDelete={foreignKey.CascadeDelete}"
                );
            }
        }

        Console.WriteLine();
    }
}