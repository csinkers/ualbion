using System;
using System.Diagnostics.CodeAnalysis;

namespace UAlbion.Formats.Assets;

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
[SuppressMessage("", "CA2217")]
public enum PlayerClasses : ushort
{
    Pilot = 0x1,
    Scientist = 0x2,
    IskaiWarrior = 0x4,
    DjiKasMage = 0x8,
    Druid = 0x10,
    EnlightenedOne = 0x20, // aka DjiKantos
    Technician = 0x40,
    // 0x80 missing
    OquloKamulos = 0x100,
    Warrior = 0x200, // 100%

    Unused = 0xfc80,

    Iskai = DjiKasMage | IskaiWarrior,
    Magicians = OquloKamulos | EnlightenedOne | Druid | DjiKasMage,
    Humans = Unused | Pilot | Scientist |                             Druid | EnlightenedOne | Technician | OquloKamulos | Warrior,
    Anyone = Unused | Pilot | Scientist | IskaiWarrior | DjiKasMage | Druid | EnlightenedOne | Technician | OquloKamulos | Warrior,
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