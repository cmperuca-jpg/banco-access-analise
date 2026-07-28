namespace Fusion.Legacy.Analyzer.Model;

public sealed class IndexModel
{
    public string Name { get; init; } = string.Empty;

    public bool IsPrimaryKey { get; init; }

    public bool IsUnique { get; init; }

    public List<IndexColumnModel> Columns { get; } = [];
}