namespace Fusion.Legacy.Analyzer.PostgreSql;

public static class PostgresIdentifier
{
    public static string Quote(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("O identificador não foi informado.", nameof(identifier));

        return $"\"{identifier.Replace("\"", "\"\"")}\"";
    }
}
