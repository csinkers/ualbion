using System;

namespace UAlbion.Core.Visual
{
    [Flags]
    public enum EtmTileFlags : uint
    {
        FloorBackAndForth = 1,
        CeilingBackAndForth = 1 << 1,
        WallBackAndForth = 1 << 2,
        Translucent = 1 << 3,
        SelfIlluminating = 1 << 4,

        TranslucentColorMask = 0xff000000
    }
}