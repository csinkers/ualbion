using System;

namespace UAlbion.Core.Visual
{
    [Flags]
    public enum DungeonTileFlags
    {
        TextureType1 = 1,
        TextureType2 = 1 << 1,
        UsePalette = 1 << 2,
        Highlight = 1 << 3,
        RedTint = 1 << 4,
        GreenTint = 1 << 5,
        BlueTint = 1 << 6,
        Transparent = 1 << 7,
    }
}