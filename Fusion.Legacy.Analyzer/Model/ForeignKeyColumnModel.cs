namespace Fusion.Legacy.Analyzer.Model;

public sealed class ForeignKeyColumnModel
{
    public string ColumnName { get; init; } = string.Empty;

    public string ReferencedColumnName { get; init; } = string.Empty;

    public int OrdinalPosition { get; init; }
}