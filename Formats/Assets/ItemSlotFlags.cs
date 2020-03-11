using System;

namespace UAlbion.Formats.Assets
{
    [Flags]
    public enum ItemSlotFlags : byte
    {
        ExtraInfo = 1,
        Broken = 2,
        Cursed = 4,
        Unk3,
        Unk4,
        Unk5,
        Unk6,
        Unk7,
    }
}
