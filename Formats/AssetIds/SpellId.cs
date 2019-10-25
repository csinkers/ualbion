namespace UAlbion.Formats.AssetIds
{
    public enum SpellId // TODO: Add this info to a JSON file and then auto-generate the enum.
    {
        // Many of these are probably wrong, e.g. Argim currently loading as
        // FrostAvalanche + BlindingStorm, but really has FrostSplinter + BlindingSpark

        // DjiKasOffset = 0,
        ThornSnare = 0,
        Hurry = 1,
        ViewOfLife = 2,
        PoisonAntidote = 3,
        InsanityAntidote = 4,
        SicknessAntidote = 5,
        FrostSplinter = 6, 
        FrostCrystal = 7,
        FrostAvalanche = 8,
        LightHealing = 9,
        BlindingSpark = 10,
        BlindingRay = 11,
        BlindingStorm = 12,
        SleepSpores = 13,
        ThornTrap = 14,
        RemoveTrapDK = 15,
        HealParalysis = 16,
        HealIntoxication = 17,
        HealBlindness = 18,
        HealPoisoning = 19,
        Fungification = 20,
        Light = 21,

        // DruidOffset = 100,
        Berserk = 100,
        BanishDemon = 101,
        BanishDemons = 102,
        DemonExodus = 103,
        SmallFireball = 104,
        MagicShield = 105,
        Healing = 106,
        Boasting = 107,
        Shock = 108,
        Panic = 109,
        PoisonStone = 110, // ??

        // EnlightenedOneOffset = 200,
        Regeneration = 200,
        MapView = 201,
        Lifebringer = 202,
        Teleporter = 203,
        QuickWithdrawal = 204,
        Levitation = 205,
        Stone = 206,
        GoddessWrath = 207,
        Irritation = 208,
        Recuperation = 209,
        // MonsterFear = 207, ??
        // MonsterFear2 = 208, ??

        // KengetKamulosOffset = 300,
        Fireball = 300,
        LightningStrike = 301,
        FireRain = 302,
        Thunderbolt = 303,
        FireHail = 304,
        Thunderstorm = 305,
        LightningTrap = 306,
        BigLightningTrap = 307,
        LightningMine = 308,
        BigLightningMine = 309,
        StealLife = 310,
        StealMagic = 311,
        PersonalProtection = 312,
        KaulossGaze = 313,
        RemoveTrapKK = 314,

        // ZombieOffset = 500
        UnknownZombieSpell3 = 500, // Candidates: Panic, Poison Breeze, Irritation, Plague Breeze
        UnknownZombieSpell1 = 501,
        UnknownZombieSpell2 = 502,
        UnknownZombieSpell4 = 503,
    }
}
