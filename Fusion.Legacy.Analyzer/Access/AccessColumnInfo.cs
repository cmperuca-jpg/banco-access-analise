namespace Fusion.Legacy.Analyzer.Access;

public sealed class AccessColumnInfo
{
    public string TableName { get; init; } = string.Empty;
    public string ColumnName { get; init; } = string.Empty;
    public int OrdinalPosition { get; init; }
    public int DataType { get; init; }
    public string DataTypeName { get; init; } = string.Empty;
    public int? CharacterMaximumLength { get; init; }
    public int? NumericPrecision { get; init; }
    public int? NumericScale { get; init; }
    public bool IsNullable { get; init; }
    public bool IsAutoIncrement { get; init; }
}