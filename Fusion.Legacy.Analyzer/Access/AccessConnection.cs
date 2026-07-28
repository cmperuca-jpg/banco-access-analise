using System.Data.OleDb;

namespace Fusion.Legacy.Analyzer.Access;

public static class AccessConnection
{
    public static OleDbConnection Create(string databasePath)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
            throw new ArgumentException("O caminho do banco não foi informado.");

        if (!File.Exists(databasePath))
            throw new FileNotFoundException(
                "Banco Access não encontrado.",
                databasePath
            );

        var connectionString =
            $"Provider=Microsoft.ACE.OLEDB.12.0;" +
            $"Data Source={databasePath};" +
            $"Persist Security Info=False;";

        return new OleDbConnection(connectionString);
    }
}