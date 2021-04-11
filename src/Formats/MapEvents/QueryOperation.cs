namespace UAlbion.Formats.MapEvents
{
    public enum QueryOperation : byte
    {
        IsTrue = 0,             // NZ >0
        LessThanOrEqual = 1,    // LE <=
        NotEqual = 2,           // NE !=
        Equals = 3,             // EQ ==
        GreaterThanOrEqual = 4, // GE >=
        GreaterThan = 5,        // GT >
        OpUnk6 = 6              // U6 ??
    }
}
