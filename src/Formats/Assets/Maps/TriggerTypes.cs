using System;

namespace UAlbion.Formats.Assets.Maps;

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

/*
    // ???
    Unk16        = 1 << 0, // 0x0001 Almost always set
    Unk17        = 1 << 1, // 0x0002 264 times
    Unk18        = 1 << 2, // 0x0004 18 times
    Unk19        = 1 << 3, // 0x0008 136 times
    Unk20        = 1 << 4, // 0x0010 Only used once
    Unk21        = 1 << 5, // 0x0020 Only used once
    Unk22        = 1 << 6, // 0x0040 Only used once - setting across the board results in instant Rainer when starting a game
    Unk23        = 1 << 7, // 0x0080 Only used once

    // ???
    Unk24        = 1 << 8, // 0x0100 Only used once
    Unk25        = 1 << 9, // 0x0200 Only used once
    Unk26        = 1 << 10, // 0x0400 Only used once
    Unk27        = 1 << 11, // 0x0800 Used 14 times. Has contact event?
    Unk28        = 1 << 12, // 0x1000 Only used once
    Unk29        = 1 << 13, // 0x2000 Only used once
    Unused30     = 1 << 14, // 0x4000
    Unused31     = 1U << 15, //  8000

    /*
     Instant Rainer:
        002...
        00A
        FFA
     */