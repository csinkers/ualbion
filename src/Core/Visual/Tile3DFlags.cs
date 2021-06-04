using System;

namespace UAlbion.Core.Visual
{
    [Flags]
    public enum Tile3DFlags
    {
        FloorBackAndForth = 1,
        CeilingBackAndForth = 1 << 1,
        WallBackAndForth = 1 << 2,
    }
}