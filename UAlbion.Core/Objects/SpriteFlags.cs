using System;

namespace UAlbion.Core.Objects
{
    [Flags]
    public enum SpriteFlags : int
    {
        NoTransform = 1,
        Highlight = 2,
        UsePalette = 4,
        OnlyEvenFrames = 8
    }
}