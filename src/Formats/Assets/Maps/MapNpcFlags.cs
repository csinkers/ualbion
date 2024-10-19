using System;

namespace UAlbion.Formats.Assets.Maps;

#pragma warning disable CA1069 // Enums values should not be duplicated
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
    Type1  = 0x1, // 2 bit value
    Type2  = 0x2, // 0 = PartyGfx; 1,2,3 = NpcGfx (2D levels)
    Type4  = 0x4, //
    MoveA1 = 0x4, // 2 bit movement type, used if MapFlags.NpcMovementMode is not set
    MoveA2 = 0x8, // part of movement type in V1, an unknown flag in V2

    SimpleMsg = 0x10, // When talked to, shows a simple msg (i.e. interprets Id as a MapText sub-id). When not set, Id = EventSetId.
    Unk5      = 0x20, // 277 times - IconPriority?
    NoClip    = 0x40, // WaveAnim?
    Unused7   = 0x80, // ?? - 2 bit 'icon height'?

    // Movement (used if MapFlags.NpcMovementMode is set)
    MoveB1 = 0x100, // part of icon height?
    MoveB2 = 0x200, // async anim?
    MoveB4 = 0x400, // random anim?
    MoveB8 = 0x800, // map graphics?

    Unused12 = 0x1000,
    Unused13 = 0x2000,
    Unused14 = 0x4000,
    Unused15 = 0x8000,
}
#pragma warning restore CA1069 // Enums values should not be duplicated
