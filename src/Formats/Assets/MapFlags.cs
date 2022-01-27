using System;

namespace UAlbion.Formats.Assets;

[Flags]
public enum MapFlags : ushort
{
    SubModeMask = SubMode1 | SubMode2,
    RestModeMask = RestMode1 | RestMode2,

    SubMode1        =    0x1,
    SubMode2        =    0x2,
    RestMode1       =    0x4,
    RestMode2       =    0x8,
    TorontoAutomap  =   0x10,
    Unused20        =   0x20,
    Unused40        =   0x40,
    Unused80        =   0x80,
    Unused100       =  0x100,
    Unused200       =  0x200,
    Unused400       =  0x400,
    Unused800       =  0x800,
    Unused1000      = 0x1000,
    V2NpcData       = 0x2000, // false = use NPC flag bits 2 & 3 for movement, true = use 8,9,10.
    ExtraNpcs       = 0x4000, // true  = 96 NPCs, false = 32
    Unk8000         = 0x8000 
}

