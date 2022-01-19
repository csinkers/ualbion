namespace UAlbion.Formats.MapEvents;

public enum QueryOperation : byte
{
    AlwaysFalse = 0,        // Always false
    LessThanOrEqual = 1,    // LE <=
    LessThan = 2,           // LT <
    Equals = 3,             // EQ ==
    GreaterThanOrEqual = 4, // GE >=
    GreaterThan = 5,        // GT >
    AlwaysFalse2 = 6,
}