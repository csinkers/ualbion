using System;

namespace UAlbion.Formats.Assets.Maps;

[Flags]
public enum NpcFlags : byte
{
    None = 0,
    Unk1 = 0x1,
    Unk2 = 0x2,
    Unk4 = 0x4, // Motion related?
    Unk8 = 0x8,
    Unk10 = 0x10,
    SimpleMsg = 0x20,
    Unk40 = 0x40,
    Unk80 = 0x80,
}