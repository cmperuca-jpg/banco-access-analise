namespace Fusion.Legacy.Analyzer.Access;

public sealed class AccessRelationInfo
{
    public string RelationName { get; init; } = string.Empty;
    public string PrimaryTable { get; init; } = string.Empty;
    public string PrimaryColumn { get; init; } = string.Empty;
    public string ForeignTable { get; init; } = string.Empty;
    public string ForeignColumn { get; init; } = string.Empty;
    public int OrdinalPosition { get; init; }
}