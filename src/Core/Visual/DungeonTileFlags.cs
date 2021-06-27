using System;

namespace UAlbion.Core.Visual
{
    [Flags]
    public enum DungeonTileFlags : uint
    {
        TextureTypeFloor = 0x0,
        TextureTypeCeiling = 0x1,
        TextureTypeWall = 0x2,
        TextureTypeMask = 0x3,

        UsePalette = 0x4,
        Highlight = 0x8,
        RedTint = 0x10,
        GreenTint = 0x20,
        BlueTint = 0x40,
        Transparent = 0x80,
        NoTexture = 0x100,
    }
}