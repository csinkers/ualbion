using System;

namespace UAlbion.Formats.Assets.Maps;

public enum NpcType
{
    Party = 0,      // PartyGfx, fires Id as party member event set (i.e. set id Id+980)
    Npc = 1,        // NpcGfx,   fires Id as EventSet
    Monster1 = 2,   // NpcGfx,   nothing
    Monster2 = 3,   // NpcGfx,   nothing
}

public enum NpcMoveA
{
    FollowWaypoints = 0,
    RandomWander = 1,
    Stationary = 2,
    ChaseParty = 3,
}

[Flags]
public enum NpcFlags : uint
{
    TypeMask = Type1 | Type2,
    MoveAMask = MoveA1 | MoveA2,
    MoveBMask = MoveB1 | MoveB2 | MoveB4 | MoveB8,
    UnusedMask = Unused7 | Unused12 | Unused13 | Unused14 | Unused15 | Unused30 | Unused31,
    MiscMask = ~(TypeMask | MoveAMask | MoveBMask),

    // Flags
    Type1         = 1 <<  0, // 0x00000001 2 bit value
    Type2         = 1 <<  1, // 0x00000002 0 = PartyGfx; 1,2,3 = NpcGfx (2D levels)
    MoveA1        = 1 <<  2, // 0x00000004 Wander randomly, don't emit waypoints
    MoveA2        = 1 <<  3, // 0x00000008 Stay put, don't emit waypoints (4+8 = follow party)
    SimpleMsg     = 1 <<  4, // 0x00000010 When talked to, shows a simple msg (i.e. interprets Id as a MapText sub-id). When not set, Id = EventSetId.
    Unk5          = 1 <<  5, // 0x00000020 277 times
    Unk6          = 1 <<  6, // 0x00000040 84 times
    Unused7       = 1 <<  7, // 0x00000080 ??

    // Movement
    MoveB1         = 1 <<  8, // 0x00000100 ??
    MoveB2         = 1 <<  9, // 0x00000200 ??
    MoveB4         = 1 << 10, // 0x00000400 ??
    MoveB8         = 1 << 11, // 0x00000800 ??

    Unused12      = 1 << 12, // 0x00001000 ??
    Unused13      = 1 << 13, // 0x00002000 ??
    Unused14      = 1 << 14, // 0x00004000 ??
    Unused15      = 1 << 15, // 0x00008000 ??

    // ???
    Unk16         = 1 << 16, // 0x00010000 Almost always set
    Unk17         = 1 << 17, // 0x00020000 264 times
    Unk18         = 1 << 18, // 0x00040000 18 times
    Unk19         = 1 << 19, // 0x00080000 136 times
    Unk20         = 1 << 20, // 0x00100000 Only used once
    Unk21         = 1 << 21, // 0x00200000 Only used once
    Unk22         = 1 << 22, // 0x00400000 Only used once - setting across the board results in instant Rainer when starting a game
    Unk23         = 1 << 23, // 0x00800000 Only used once

    // ???
    Unk24         = 1 << 24, // 0x01000000 Only used once
    Unk25         = 1 << 25, // 0x02000000 Only used once
    Unk26         = 1 << 26, // 0x04000000 Only used once
    Unk27         = 1 << 27, // 0x08000000 Used 14 times. Has contact event?
    Unk28         = 1 << 28, // 0x10000000 Only used once
    Unk29         = 1 << 29, // 0x20000000 Only used once
    Unused30      = 1 << 30, // 0x40000000
    Unused31      = 1U << 31, //  80000000

    /*
     Instant Rainer:
        002...
        00A
        FFA
     */
}
