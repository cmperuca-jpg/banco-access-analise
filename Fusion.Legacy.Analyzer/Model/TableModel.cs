namespace Fusion.Legacy.Analyzer.Model;

public sealed class TableModel
{
    public string Name { get; init; } = string.Empty;

    public List<ColumnModel> Columns { get; } = [];

    public List<IndexModel> Indexes { get; } = [];

    public List<ForeignKeyModel> ForeignKeys { get; } = [];
}