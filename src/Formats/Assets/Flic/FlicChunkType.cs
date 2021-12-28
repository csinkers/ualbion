namespace UAlbion.Formats.Assets.Flic;

public enum FlicChunkType : ushort
{
    Palette8Bit = 4,
    DeltaWordOrientedRle = 7,
    Palette6Bit = 11,
    DeltaByteOrientedRle = 12,
    BlackFrameData = 13,
    FullByteOrientedRle = 15,
    FullUncompressed = 16,
    Thumbnail = 18,
    FullPixelOrientedRle = 25,
    FullUncompressed2 = 26,
    DeltaPixelOrientedRle = 27,
    FrameLabel = 31,
    BitmapMask = 32,
    MultilevelMask = 33,
    Segment = 34,
    KeyImage = 35,
    KeyPalette = 36,
    Region = 37,
    Wave = 38,
    UserString = 39,
    RegionMask = 40,
    ExtendedFrameLabel = 41,
    ScanlineDeltaShifts = 42,
    PathMap = 43,

    Prefix = 0xF100,
    Script = 0xF1E0,
    Frame = 0xF1FA,
    SegmentTable = 0xF1FB,
    HuffmanTable = 0xF1FC
}