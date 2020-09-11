using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UAlbion.Formats.Assets
{
    [JsonConverter(typeof(StringEnumConverter))]
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
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SpellClasses : byte
    {
        DjiKas       =  1,
        DjiKantos    =  2, // Enlightened ones
        Druid        =  4,
        OquloKamulos =  8,
        Unk4         = 16,
        ZombieMagic  = 32,
    }
}
