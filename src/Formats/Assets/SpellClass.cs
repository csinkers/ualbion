using System;

namespace UAlbion.Formats.Assets;

public enum SpellClass : byte
{
    DjiKas       = 0, // Sira etc
    DjiKantos    = 1, // Enlightened ones
    Druid        = 2, // Mellthas
    OquloKamulos = 3, // Khunag
    Unk4         = 4, // Unused
    ZombieMagic  = 5, // Monsters only
    Unk6         = 6, // Unused
}

[Flags]
public enum SpellClasses : byte
{
    None         =  0,
    DjiKas       =  1,
    DjiKantos    =  2, // Enlightened ones
    Druid        =  4,
    OquloKamulos =  8,
    Unk4         = 16,
    ZombieMagic  = 32,
    Unk6         = 64,
}

public static class SpellClassExtensions
{
    public static SpellClasses ToFlag(this SpellClass c)
    {
        return c switch
        {
            SpellClass.DjiKas       => SpellClasses.DjiKas,
            SpellClass.DjiKantos    => SpellClasses.DjiKantos,
            SpellClass.Druid        => SpellClasses.Druid,
            SpellClass.OquloKamulos => SpellClasses.OquloKamulos,
            SpellClass.Unk4         => SpellClasses.Unk4,
            SpellClass.ZombieMagic  => SpellClasses.ZombieMagic,
            SpellClass.Unk6         => SpellClasses.Unk6,
            _ => throw new ArgumentOutOfRangeException(nameof(c), c, null)
        };
    }
}
