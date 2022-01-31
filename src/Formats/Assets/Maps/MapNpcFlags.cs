using System;

namespace UAlbion.Formats.Assets.Maps;

[Flags]
public enum MapNpcFlags : ushort
{
    TypeMaskV1 = Type1 | Type2,
    TypeMaskV2 = Type1 | Type2 | Type4,
    MoveMaskV1 = MoveA1 | MoveA2,
    MoveMaskV2 = MoveB1 | MoveB2 | MoveB4 | MoveB8,
    UnusedMask = Unused7 | Unused12 | Unused13 | Unused14 | Unused15,
    MiscMaskV1 = unchecked((ushort)~(TypeMaskV1 | MoveMaskV1)),
    MiscMaskV2 = unchecked((ushort)~(TypeMaskV2 | MoveMaskV2)),

    // Flags
    Type1 = 1 << 0, // 0x00000001 2 bit value
    Type2 = 1 << 1, // 0x00000002 0 = PartyGfx; 1,2,3 = NpcGfx (2D levels)
    Type4 = 1 << 2, // 0x00000004 
    MoveA1 = 1 << 2, // 0x00000004 2 bit movement type, used if MapFlags.NpcMovementMode is not set
    MoveA2 = 1 << 3, // 0x00000008 part of movement type in V1, an unknown flag in V2

    Unk3      = 1 << 3,
    SimpleMsg = 1 << 4, // 0x00000010 When talked to, shows a simple msg (i.e. interprets Id as a MapText sub-id). When not set, Id = EventSetId.
    Unk5      = 1 << 5, // 0x00000020 277 times
    NoClip    = 1 << 6,
    Unused7   = 1 << 7, // 0x00000080 ??

    // Movement (used if MapFlags.NpcMovementMode is set)
    MoveB1 = 1 << 8,  // 0x00000100 
    MoveB2 = 1 << 9,  // 0x00000200 
    MoveB4 = 1 << 10, // 0x00000400 
    MoveB8 = 1 << 11, // 0x00000800 

    Unused12 = 1 << 12, // 0x00001000 ??
    Unused13 = 1 << 13, // 0x00002000 ??
    Unused14 = 1 << 14, // 0x00004000 ??
    Unused15 = 1 << 15, // 0x00008000 ??
}