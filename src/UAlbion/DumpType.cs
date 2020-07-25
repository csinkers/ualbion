using System;

namespace UAlbion
{
    [Flags]
    public enum DumpType
    {
        Characters  = 1 << 0,
        Chests      = 1 << 1,
        CoreSprites = 1 << 2,
        EventSets   = 1 << 3,
        Items       = 1 << 4,
        MapEvents   = 1 << 5,
        Maps        = 1 << 6,
        Spells      = 1 << 7,
        ThreeDMaps  = 1 << 8,

        All = 0x7fffffff
    }
}