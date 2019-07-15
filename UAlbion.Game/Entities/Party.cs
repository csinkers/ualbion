using System.Collections.Generic;

namespace UAlbion.Game.Entities
{
    enum Spell
    {
        // DjiKas
        ThornSnare,
        Hurry,
        ViewOfLife,
        FrostSplinter,
        FrostCrystal,
        FrostAvalanche,
        LightHealing,
        BlindingSpark,
        BlindingRay,
        BlindingStorm,
        SleepSpores,
        ThornTrap,
        RemoveTrap,
        HealIntoxication,
        HealBlindness,
        HealPoisoning,
        Fungification,
        Light,

        // Druid
        Berserk,
        BanishDemon,
        BanishDemons,
        DemonExodus,
        SmallFireball,
        MagicShield,
        Healing,
        Boasting,
        Shock,
        Panic,

        // Enlightened One
        Regeneration,
        MapView,
        Teleporter,
        // Healing, // Shared or dupe name?
        QuickWithdrawal,
        Irritation,
        Recuperation,

        // Kenget Kamulos
        Fireball,
        LightningStrike,
        FireRain,
        Thunderbolt,
        FireHail,
        Thunderstorm,
        PersonalProtection
    }

    enum Direction
    {
        North, East, South, West
    }

    public class Party
    {
        const int PositionHistoryCount = 40;

        readonly IList<Player> _players = new List<Player>();
        readonly (int,int)[] _positions = new (int,int)[PositionHistoryCount];
        Direction _facing;
        int _activePlayer;
        bool _hasClock;
        bool _hasProximityDetector;
    }
}
