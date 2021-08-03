using System;

namespace UAlbion.Formats.Assets
{
    [Flags]
    public enum SpellEnvironments : byte
    {
        // The first 4 always appear together: reversing of
        // the exe will be required to see which is actually which
        Indoors   = 1 << 0, 
        Outdoors  = 1 << 1,
        Dungeon   = 1 << 2,
        Inventory = 1 << 3,

        Unk4   = 1 << 4,
        Combat = 1 << 5,
        Unk6   = 1 << 6,
        Unk7   = 1 << 7,
    }
}
