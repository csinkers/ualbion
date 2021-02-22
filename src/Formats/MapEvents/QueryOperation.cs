namespace UAlbion.Formats.MapEvents
{
    public enum QueryOperation : byte
    {
        IsTrue = 0,             // NZ >0
        NotEqual = 1,           // NE !=
        OpUnk2 = 2,             // U2 ??
        Equals = 3,             // EQ ==
        GreaterThanOrEqual = 4, // GE >=
        GreaterThan = 5,        // GT >
        OpUnk6 = 6              // U6 ??
    }
}
