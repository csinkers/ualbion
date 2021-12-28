using System;

namespace UAlbion.Formats.Assets;

[Flags]
public enum ItemSlotFlags : byte
{
    ExtraInfo = 0x1,
    Broken = 0x2,
    Cursed = 0x4,
    Unk3 = 0x8,
    Unk4 = 0x10,
    Unk5 = 0x20,
    Unk6 = 0x40,
    Unk7 = 0x80,
}