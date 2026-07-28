namespace Fusion.Legacy.Analyzer.Access;

public sealed class AccessIndexInfo
{
    public string TableName { get; init; } = string.Empty;
    public string IndexName { get; init; } = string.Empty;
    public string ColumnName { get; init; } = string.Empty;
    public int OrdinalPosition { get; init; }
    public bool IsPrimaryKey { get; init; }
    public bool IsUnique { get; init; }
}