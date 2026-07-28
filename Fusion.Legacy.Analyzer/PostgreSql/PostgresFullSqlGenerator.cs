using System.Text;

namespace Fusion.Legacy.Analyzer.PostgreSql;

public static class PostgresFullSqlGenerator
{
    public static void WriteFullFile(
        string schemaPath,
        string dataPath,
        string outputPath)
    {
        if (string.IsNullOrWhiteSpace(schemaPath))
            throw new ArgumentException("O caminho do esquema não foi informado.", nameof(schemaPath));

        if (string.IsNullOrWhiteSpace(dataPath))
            throw new ArgumentException("O caminho dos dados não foi informado.", nameof(dataPath));

        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("O caminho de saída não foi informado.", nameof(outputPath));

        var fullSchemaPath = Path.GetFullPath(schemaPath);
        var fullDataPath = Path.GetFullPath(dataPath);
        var fullOutputPath = Path.GetFullPath(outputPath);

        if (!File.Exists(fullSchemaPath))
            throw new FileNotFoundException("O arquivo de esquema não foi encontrado.", fullSchemaPath);

        if (!File.Exists(fullDataPath))
            throw new FileNotFoundException("O arquivo de dados não foi encontrado.", fullDataPath);

        var directory = Path.GetDirectoryName(fullOutputPath);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        using var writer = new StreamWriter(
            fullOutputPath,
            false,
            new UTF8Encoding(false),
            bufferSize: 1024 * 1024);

        writer.WriteLine("-- Gerado por Fusion.Legacy.Analyzer");
        writer.WriteLine("-- Migração completa: estrutura e dados");
        writer.WriteLine("-- O arquivo contém duas transações: esquema e dados.");
        writer.WriteLine();

        CopyFile(writer, fullSchemaPath);
        writer.WriteLine();
        writer.WriteLine();
        CopyFile(writer, fullDataPath);
    }

    private static void CopyFile(TextWriter writer, string sourcePath)
    {
        using var reader = new StreamReader(sourcePath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        while (reader.ReadLine() is { } line)
            writer.WriteLine(line);
    }
}
