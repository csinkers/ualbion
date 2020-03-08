using System;

namespace UAlbion.Formats.Assets
{
    public enum PlayerClass : byte
    {
        Pilot = 0,
        Scientist = 1,
        IskaiWarrior = 2,
        DjiKasMage = 3,
        Druid = 4,
        EnlightenedOne = 5,
        Technician = 6,
        OquloKamulos = 8,
        Warrior = 9,
        Monster = 31
    }

    [Flags]
    public enum PlayerClassMask : ushort
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

    public static class PlayerClassExtensions
    {
        public static bool IsAllowed(this PlayerClassMask mask, PlayerClass playerClass) =>
            playerClass switch
            {
                PlayerClass.Pilot          => 0 != (mask & PlayerClassMask.Pilot),
                PlayerClass.Scientist      => 0 != (mask & PlayerClassMask.Scientist),
                PlayerClass.IskaiWarrior   => 0 != (mask & PlayerClassMask.IskaiWarrior),
                PlayerClass.DjiKasMage     => 0 != (mask & PlayerClassMask.DjiKasMage),
                PlayerClass.Druid          => 0 != (mask & PlayerClassMask.Druid),
                PlayerClass.EnlightenedOne => 0 != (mask & PlayerClassMask.EnlightenedOne),
                PlayerClass.Technician     => 0 != (mask & PlayerClassMask.Technician),
                PlayerClass.OquloKamulos   => 0 != (mask & PlayerClassMask.OquloKamulos),
                PlayerClass.Warrior        => 0 != (mask & PlayerClassMask.Warrior),
                _ => false
            };
    }
}
