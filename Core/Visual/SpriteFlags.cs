using System;

namespace UAlbion.Core.Visual
{
    [Flags]
    public enum SpriteFlags : int
    {
        NoTransform    = 0x1,
        Highlight      = 0x2,
        UsePalette     = 0x4,
        OnlyEvenFrames = 0x8,
        RedTint        = 0x10,
        GreenTint      = 0x20,
        BlueTint       = 0x40,
        Transparent    = 0x80,
        FlipVertical   = 0x100,
        Floor          = 0x200,
        Billboard      = 0x400,
        DropShadow     = 0x800
    }
}
