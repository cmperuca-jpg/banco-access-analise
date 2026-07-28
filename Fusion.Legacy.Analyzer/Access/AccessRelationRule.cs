namespace Fusion.Legacy.Analyzer.Access;

public sealed class AccessRelationRule
{
    public string RelationName { get; init; } = string.Empty;

    public bool CascadeUpdate { get; init; }

    public bool CascadeDelete { get; init; }

    public bool LeftTable { get; init; }

    public bool RightTable { get; init; }
}