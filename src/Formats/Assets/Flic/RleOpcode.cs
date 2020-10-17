namespace UAlbion.Formats.Assets.Flic
{
    public enum RleOpcode
    {
        Packets = 0,
        Undefined = 1,
        StoreLowByteInLastPixel = 2,
        LineSkipCount = 3, // Take absolute value first
    }
}
