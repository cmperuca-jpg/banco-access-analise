namespace Fusion.Legacy.Analyzer.Model;

public sealed class ForeignKeyModel
{
    public string Name { get; init; } = string.Empty;

    public string ReferencedTable { get; init; } = string.Empty;

    public bool CascadeUpdate { get; init; }

    public bool CascadeDelete { get; init; }

    public List<ForeignKeyColumnModel> Columns { get; } = [];
}