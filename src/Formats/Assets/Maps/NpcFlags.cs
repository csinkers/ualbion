using System;

namespace UAlbion.Formats.Assets.Maps
{
    [Flags]
    public enum NpcFlags : byte
    {
        Wander    = 1 << 0, // If not will always move towards the party
        IsMonster = 1 << 1,
        Unk2      = 1 << 2,
        Unk3      = 1 << 3, // Has contact event?
        Unk4      = 1 << 4,
        Unk5      = 1 << 5,
        Unk6      = 1 << 6,
        Unk7      = 1 << 7,
    }
}
