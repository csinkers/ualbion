using System;

namespace UAlbion.Core.Objects
{
    [Flags]
    public enum SpriteFlags : byte
    {
        TrueColour = 1,
        NoTransform = 2,
        Highlight = 4,
    }
}