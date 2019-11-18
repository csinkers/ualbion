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
                PlayerClass.Pilot => mask.HasFlag(PlayerClassMask.Pilot),
                PlayerClass.Scientist => mask.HasFlag(PlayerClassMask.Scientist),
                PlayerClass.IskaiWarrior => mask.HasFlag(PlayerClassMask.IskaiWarrior),
                PlayerClass.DjiKasMage => mask.HasFlag(PlayerClassMask.DjiKasMage),
                PlayerClass.Druid => mask.HasFlag(PlayerClassMask.Druid),
                PlayerClass.EnlightenedOne => mask.HasFlag(PlayerClassMask.EnlightenedOne),
                PlayerClass.Technician => mask.HasFlag(PlayerClassMask.Technician),
                PlayerClass.OquloKamulos => mask.HasFlag(PlayerClassMask.OquloKamulos),
                PlayerClass.Warrior => mask.HasFlag(PlayerClassMask.Warrior),
                _ => false
            };
    }
}
