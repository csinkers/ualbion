using UAlbion.Api.Visual;

namespace UAlbion.Formats.Assets.Maps;

public enum TileLayer : byte // Upper nibble of first byte
{
    Normal = 0, // Most floors, low and centre EW walls
    Layer1 = 2, // Mid EW walls, Overlay1
    Layer2 = 4, // 
    Layer3 = 6, // NS walls + Overlay3
    Unk8 = 8,
    Unk10 = 10,
    Unk12 = 12, // Only used for overlay
    Unk14 = 14, // Only used for overlay

    Unused1 = 1,
    Unused3 = 3,
    Unused5 = 5,
    Unused7 = 7,
    Unused9 = 9,
    Unused11 = 11,
    Unused13 = 13,
    Unused15 = 15,
}

public static class TileLayerExtensions
{
    public static int ToDepthOffset(this TileLayer layer)
    {
        int adjustment;
        switch ((int)layer & 0x7)
        {
            case (int)TileLayer.Normal: adjustment = DepthUtil.NormalAdjustment; break;
            case (int)TileLayer.Layer1: adjustment = DepthUtil.Layer1Adjustment; break;
            case (int)TileLayer.Layer2: adjustment = DepthUtil.Layer2Adjustment; break;
            case (int)TileLayer.Layer3: adjustment = DepthUtil.Layer3Adjustment; break;
            default: adjustment = 0; break;
        }
        return adjustment;
    }
}