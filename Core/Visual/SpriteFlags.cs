using System;

namespace UAlbion.Core.Visual
{
    [Flags]
    public enum SpriteFlags : int
    {
        NoTransform = 1,
        Highlight = 2,
        UsePalette = 4,
        OnlyEvenFrames = 8,
        RedTint = 16,
        GreenTint = 32,
        BlueTint = 64,
        Transparent = 128,
        FlipVertical = 256,
    }
}