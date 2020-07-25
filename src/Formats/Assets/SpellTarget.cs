using System;

namespace UAlbion.Formats.Assets
{
    [Flags]
    public enum SpellTarget : byte
    {
        Party     = 1 << 0,
        Unk1      = 1 << 1,
        DeadParty = 1 << 2,
        Monsters1 = 1 << 3,
        Monsters2 = 1 << 4,
        Monsters3 = 1 << 5,
        Unk6      = 1 << 6,
        MapTile   = 1 << 7,
    }
}
