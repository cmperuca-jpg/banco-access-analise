namespace Fusion.Legacy.Analyzer.Model;

public sealed class ColumnModel
{
    public string Name { get; init; } = string.Empty;

    public int AccessType { get; init; }

    public string AccessTypeName { get; init; } = string.Empty;

    public int? CharacterMaximumLength { get; init; }

    public int? NumericPrecision { get; init; }

    public int? NumericScale { get; init; }

    public bool IsNullable { get; init; }

    public bool IsAutoIncrement { get; init; }

    public int OrdinalPosition { get; init; }
}
