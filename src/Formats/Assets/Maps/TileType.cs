using UAlbion.Api;

namespace UAlbion.Formats.Assets.Maps
{
    public enum TileType : byte
    {
        Normal = 0,   // Standard issue
        Underlay1 = 1, // Underlay
        Underlay2 = 2, // Underlay, celtic floors, toilet seats, random square next to pumps, top shelves of desks
        Underlay3 = 3, // Unused
        Overlay1 = 4, // Overlay
        Overlay2 = 5, // Overlay, only on continent maps?
        Overlay3 = 6, // Overlay
        Unk7 = 7,     // Overlay used on large plants on continent maps.
        Unk8 = 8,     // Underlay used on OkuloKamulos maps around fire
        Unk12 = 12,   // Overlay used on OkuloKamulos maps for covered fire / grilles
        Unk14 = 14,   // Overlay used on OkuloKamulos maps for open fire

        Unused9 = 9,
        Unused10 = 10,
        Unused11 = 11,
        Unused13 = 13,
        Unused15 = 15,
    }

    public static class TileTypeExtensions
    {
        public static int ToDepthOffset(this TileType type)
        {
            int typeAdjust;
            switch ((int)type & 0x7)
            {
                case (int)TileType.Normal: typeAdjust = DepthUtil.TypeNormalAdjustment; break;
                case (int)TileType.Overlay1: typeAdjust = DepthUtil.TypeOverlay1Adjustment; break;
                case (int)TileType.Overlay2: typeAdjust = DepthUtil.TypeOverlay2Adjustment; break;
                case (int)TileType.Overlay3: typeAdjust = DepthUtil.TypeOverlay3Adjustment; break;
                case (int)TileType.Unk7: typeAdjust = 0; break;
                default: typeAdjust = 0; break;
            }
            return typeAdjust;
        }
    }
}
