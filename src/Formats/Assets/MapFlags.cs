using System;

namespace UAlbion.Formats.Assets;

[Flags]
public enum MapFlags : ushort
{
    Unlit           =    0x1, // All NPCs dark when debug mode off
    Unk2            =    0x2,
    CanRest1        =    0x4, // Can rest instead of waiting
    CanRest2        =    0x8, // Can rest (2)
    Unk10           =   0x10,
    Unk20           =   0x20,
    Unk40           =   0x40,
    Unk80           =   0x80,
    Unk100          =  0x100,
    Unk200          =  0x200,
    Unk400          =  0x400,
    Unk800          =  0x800,
    Unk1000         = 0x1000,
    NpcMovementMode = 0x2000, // false = use NPC flag bits 2 & 3 for movement, true = use 8,9,10.
    ExtraNpcs       = 0x4000, // true  = 96 NPCs, false                             = 32
    Unk8000         = 0x0008
}
