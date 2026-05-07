using System;

namespace UAlbion.Formats.Assets;

public enum SpellEffectType : byte
{
    Unknown = 0,
    HealHP = 1,
    CureIntoxicated = 2,
    CurePoisoned = 3,
    CureDiseased = 4,
    CureAsleep = 5,
    CureParalysed = 6,
    CureBlind = 7,
    CureInsane = 8,
    CurePanicking = 9,
    CureIrritated = 10,
    CureExhausted = 11,
    CureAllConditions = 12,
    Resurrect = 13,
    Damage = 14,
    Blink = 15,
    Irritate = 16,
    HealAllHP = 17,
    Recuperation = 18,
    Regeneration = 19,
    SetBlind = 20,
    FrostDamage = 21,
    StealLife = 22,
    StealMagic = 23,
}
