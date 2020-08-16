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
    public enum PlayerClasses : ushort
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
        EveryoneMask = 0xffff,
    }

    public static class PlayerClassExtensions
    {
        public static bool IsAllowed(this PlayerClasses mask, PlayerClass playerClass) =>
            playerClass switch
            {
                PlayerClass.Pilot          => 0 != (mask & PlayerClasses.Pilot),
                PlayerClass.Scientist      => 0 != (mask & PlayerClasses.Scientist),
                PlayerClass.IskaiWarrior   => 0 != (mask & PlayerClasses.IskaiWarrior),
                PlayerClass.DjiKasMage     => 0 != (mask & PlayerClasses.DjiKasMage),
                PlayerClass.Druid          => 0 != (mask & PlayerClasses.Druid),
                PlayerClass.EnlightenedOne => 0 != (mask & PlayerClasses.EnlightenedOne),
                PlayerClass.Technician     => 0 != (mask & PlayerClasses.Technician),
                PlayerClass.OquloKamulos   => 0 != (mask & PlayerClasses.OquloKamulos),
                PlayerClass.Warrior        => 0 != (mask & PlayerClasses.Warrior),
                _ => false
            };
    }
}
