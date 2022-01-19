using System;

namespace UAlbion.Formats.Assets;

[Flags]
public enum MapFlags : ushort
{
    Unk1 = 1,
    Unk2 = 1 << 1,
    Unk4 = 1 << 2,
    Unk8 = 1 << 3,
    Unk10 = 1 << 4,
    Unk20 = 1 << 5,
    Unk40 = 1 << 6,
    Unk80 = 1 << 7,
    Unk100 = 1 << 8,
    Unk200 = 1 << 9,
    Unk400 = 1 << 10,
    Unk800 = 1 << 11,
    Unk1000 = 1 << 12,
    NpcMovementMode = 1 << 13, // false = use NPC flag bits 2 & 3 for movement, true = 8 & 9
    ExtraNpcs = 1 << 14, // true = 96 NPCs, false = 32
    Unk8000 = 1 << 15
}