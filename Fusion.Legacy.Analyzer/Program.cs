using Fusion.Legacy.Analyzer.Access;
using Fusion.Legacy.Analyzer.Builders;
using Fusion.Legacy.Analyzer.PostgreSql;

Console.WriteLine(
    $"Sistema operacional 64 bits: {Environment.Is64BitOperatingSystem}"
);

Console.WriteLine(
    $"Processo 64 bits: {Environment.Is64BitProcess}"
);

Console.Write("Informe o caminho completo do arquivo ACCDB ou MDB: ");
var databasePath = Console.ReadLine();

if (string.IsNullOrWhiteSpace(databasePath))
{
    Console.WriteLine("Caminho não informado.");
    return;
}

try
{
    using var connection = AccessConnection.Create(databasePath);

    var databaseModel = AccessDatabaseModelBuilder.Build(connection);

    DatabaseModelPrinter.Print(databaseModel);

    var identityColumns = databaseModel.Tables
        .SelectMany(table => table.Columns
            .Where(column => column.IsAutoIncrement)
            .Select(column => $"{table.Name}.{column.Name}"))
        .ToList();

    Console.WriteLine();
    Console.WriteLine($"Colunas AutoIncrement detectadas: {identityColumns.Count}");

    foreach (var identityColumn in identityColumns)
        Console.WriteLine($"  {identityColumn}");

    var schemaOutputPath = Path.Combine(
        Environment.CurrentDirectory,
        "postgresql-schema.sql"
    );

    var dataOutputPath = Path.Combine(
        Environment.CurrentDirectory,
        "postgresql-data.sql"
    );

    var fullOutputPath = Path.Combine(
        Environment.CurrentDirectory,
        "postgresql-full.sql"
    );

    PostgresSqlGenerator.WriteSchemaFile(
        databaseModel,
        schemaOutputPath
    );

    Console.WriteLine(new string('=', 80));
    Console.WriteLine("EXPORTAÇÃO DOS DADOS");
    Console.WriteLine(new string('=', 80));

    PostgresDataSqlGenerator.WriteDataFile(
        connection,
        databaseModel,
        dataOutputPath,
        message => Console.WriteLine(message)
    );

    PostgresFullSqlGenerator.WriteFullFile(
        schemaOutputPath,
        dataOutputPath,
        fullOutputPath
    );

    Console.WriteLine();
    Console.WriteLine(new string('=', 80));
    Console.WriteLine("ARQUIVOS POSTGRESQL GERADOS");
    Console.WriteLine(new string('=', 80));
    Console.WriteLine(schemaOutputPath);
    Console.WriteLine(dataOutputPath);
    Console.WriteLine(fullOutputPath);
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("ERRO:");
    Console.WriteLine(ex);
}
