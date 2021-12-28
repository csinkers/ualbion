using System;

namespace UAlbion.Formats.Assets;

[Flags]
public enum ItemFlags : ushort
{
    Unk0      = 1, // Double battle-axe only
    PlotItem  = 1 << 1,
    Stackable = 1 << 2,
    Unk3      = 1 << 3,
    Unk4      = 1 << 4, // Spell scrolls, potions, HUD items, throwing daggers/stones and lockpicks
    Unk5      = 1 << 5,
    Unk6      = 1 << 6,
    Unk7      = 1 << 7,
    Unk8      = 1 << 8, // Dji-Cantos stone only
    Unk9      = 1 << 9,
    Cursed    = 1 << 10,
    Unk11     = 1 << 11,
    Unk12     = 1 << 12,
}