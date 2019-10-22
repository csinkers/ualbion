using System;

namespace UAlbion.Formats.Assets
{
    [Flags]
    public enum PlayerClass : ushort
    {
        Pilot = 0x1,
        Scientist = 0x2,
        IskaiWarrior = 0x4,
        DjiKasMage = 0x8,
        Druid = 0x10,
        EnlightenedOne = 0x20, // aka DjiKantos
        Technician = 0x40,
        OquloKamulos = 0x100,
        Warrior = 0x200, // 100%

        IskaiMask = 0xc,
        MagicianMask = 0x138,
        HumanMask = 0xfff3,
    }
}