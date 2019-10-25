using System;

namespace UAlbion.Formats.Assets
{
    public enum SpellClass : byte
    {
        DjiKas = 0,
        DjiKantos = 1, // Enlightened ones
        Druid = 2,
        OquloKamulos = 3,
        Unk4 = 4,
        ZombieMagic = 5,
    }

    [Flags]
    public enum SpellClassMask : byte
    {
        DjiKas = 1,
        DjiKantos = 2, // Enlightened ones
        Druid = 4,
        OquloKamulos = 8,
        Unk4 = 16,
        ZombieMagic = 32,
    }
}