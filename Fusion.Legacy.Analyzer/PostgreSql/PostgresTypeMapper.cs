using Fusion.Legacy.Analyzer.Model;

namespace Fusion.Legacy.Analyzer.PostgreSql;

public static class PostgresTypeMapper
{
    public static string Map(ColumnModel column)
    {
        ArgumentNullException.ThrowIfNull(column);

        return column.AccessType switch
        {
            2 => "smallint",                       // SmallInt
            3 => "integer",                        // Integer
            4 => "real",                           // Single
            5 => "double precision",               // Double
            6 => "numeric(19,4)",                  // Currency
            7 => "timestamp without time zone",    // Date
            11 => "boolean",                       // Boolean
            14 => MapNumeric(column),               // Decimal
            16 => "smallint",                      // TinyInt
            17 => "smallint",                      // UnsignedTinyInt
            18 => "integer",                       // UnsignedSmallInt
            19 => "bigint",                        // UnsignedInt
            20 => "bigint",                        // BigInt
            21 => "numeric(20,0)",                 // UnsignedBigInt
            72 => "uuid",                          // Guid
            128 => "bytea",                        // Binary
            129 => MapCharacter(column),            // Char
            130 => MapCharacter(column),            // WChar
            200 => MapCharacter(column),            // VarChar
            201 => "text",                         // LongVarChar / Memo
            202 => MapCharacter(column),            // VarWChar
            203 => "text",                         // LongVarWChar / Memo Unicode
            204 => "bytea",                        // VarBinary
            205 => "bytea",                        // LongVarBinary / OLE Object
            _ => throw new NotSupportedException(
                $"Tipo OLE DB não suportado: {column.AccessTypeName} ({column.AccessType}) " +
                $"na coluna '{column.Name}'.")
        };
    }

    private static string MapCharacter(ColumnModel column)
    {
        return column.CharacterMaximumLength is > 0
            ? $"character varying({column.CharacterMaximumLength.Value})"
            : "text";
    }

    private static string MapNumeric(ColumnModel column)
    {
        if (column.NumericPrecision is > 0 && column.NumericScale is >= 0)
            return $"numeric({column.NumericPrecision.Value},{column.NumericScale.Value})";

        if (column.NumericPrecision is > 0)
            return $"numeric({column.NumericPrecision.Value})";

        return "numeric";
    }
}
