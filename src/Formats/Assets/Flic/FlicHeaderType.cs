namespace UAlbion.Formats.Assets.Flic
{
    public enum FlicHeaderType : ushort
    {
        Fli = 0xAF11,
        Flc = 0xAF12,
        FlcHuffmanOrBwt = 0xAF30,
        FlcFrameShift = 0xAF31,
        FlcNon8Bit = 0xAF44,
    }
}