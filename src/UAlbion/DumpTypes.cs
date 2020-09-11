using System;

namespace UAlbion
{
    [Flags]
    public enum DumpFormats
    {
        Json      = 1 << 0, // Machine & human-readable, good for inter-operation and modding
        Text      = 1 << 1, // Dense ad-hoc text format, good for an overview.
        Png       = 1 << 2, // True-colour PNG with colours resolved at palette tick 0
        PngCycled = 1 << 3, // True-colour PNGs capturing all significant palette steps
        Bmp       = 1 << 4, // 8-bit Device-Independent Bitmap with palette copied at tick 0
        BmpCycled = 1 << 5, // 8-bit Device-Independent Bitmaps capturing all significant palette steps

        GraphicsMask = Bmp | Png | BmpCycled | PngCycled
    }
}
