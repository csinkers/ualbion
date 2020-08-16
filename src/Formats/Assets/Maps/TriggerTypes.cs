using System;

namespace UAlbion.Formats.Assets.Maps
{
    [Flags]
    public enum TriggerTypes : ushort
    {
        Normal     = 1,       //    1
        Examine    = 1 << 1,  //    2
        Manipulate = 1 << 2,  //    4
        TalkTo     = 1 << 3,  //    8
        UseItem    = 1 << 4,  //   10
        MapInit    = 1 << 5,  //   20
        EveryStep  = 1 << 6,  //   40
        EveryHour  = 1 << 7,  //   80
        EveryDay   = 1 << 8,  //  100
        Default    = 1 << 9,  //  200
        Action     = 1 << 10, //  400
        Npc        = 1 << 11, //  800
        Take       = 1 << 12, // 1000
        Unk13      = 1 << 13, // 2000
        Unk14      = 1 << 14, // 4000
        Unk15      = 1 << 15, // 8000
    }
}
