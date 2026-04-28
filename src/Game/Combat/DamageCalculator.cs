using UAlbion.Formats.Assets.Sheets;
using UAlbion.Game;

namespace UAlbion.Game.Combat;

public record DamageResult(
    int Afflicted,
    int RawDamage,
    int RawProtection,
    int RolledDamage,
    int RolledProtection,
    bool IsCritical);

// Formula source: MS-DOS/221195/ALBION/SRC/COMBAT/COMACTS.C
// Calculate_afflicted_damage, Probe_skill, Get_rnd_50_100
public static class DamageCalculator
{
    // Get_rnd_50_100: value/2 + rand(0..value/2)  → result is 50–100% of input
    // Called on both damage and protection before comparing.
    public static int GetRnd50_100(IRandom rng, int value)
    {
        if (value <= 0) return 0;
        int half = value / 2;
        return half + rng.Generate(half + 1);
    }

    // Probe_skill: rand(0..99) < skillValue.Current → success
    static bool ProbeSkill(IRandom rng, ICharacterAttribute skill)
        => rng.Generate(100) < skill.Current;

    // Calculate_afflicted_damage from COMACTS.C:
    //   damage     = Get_damage(attacker) + attacker.Strength / 25
    //                [+ spell bonus, not implemented — no spell bonuses in engine yet]
    //   protection = Get_protection(defender)
    //                [+ spell bonus, not implemented]
    //   Both are randomised to 50–100% via Get_rnd_50_100.
    //   afflicted  = max(damage - protection, 0)
    //   Critical hit: Probe_skill(CriticalChance) && defender is not a boss-type monster
    //                 → afflicted = defender.MaxHP  (instant kill)
    //
    // Get_damage(char) maps to BaseAttack + BonusAttack (intrinsic, monsters) + DisplayDamage (weapons, party).
    // Get_protection(char) maps to Combat.BaseDefense (after EffectiveSheetCalculator, includes armor).
    // The END_MONSTER flag (immune to crits) has no direct equivalent in the current
    // asset schema; we approximate it by treating Monster CharacterType as non-boss
    // for now.  DEVIATION: END_MONSTER per-monster flag not yet exposed in ICharacterSheet —
    // using CharacterType.Monster as stand-in; refine when flag is available.
    public static DamageResult CalculateAfflictedDamage(
        IRandom rng,
        IEffectiveCharacterSheet attacker,
        IEffectiveCharacterSheet defender)
    {
        int rawDamage = attacker.Combat.BaseAttack + attacker.Combat.BonusAttack
                        + attacker.DisplayDamage
                        + attacker.Attributes.Strength.Current / 25;
        int rawProtection = defender.Combat.BaseDefense;

        int damage = GetRnd50_100(rng, rawDamage);
        int protection = GetRnd50_100(rng, rawProtection);

        int afflicted = damage - protection;
        if (afflicted < 0) afflicted = 0;

        // Critical hit check
        bool isCritical = attacker.Type == CharacterType.Party
            && defender.Type != CharacterType.Party
            && ProbeSkill(rng, attacker.Skills.CriticalChance);
        if (isCritical)
        {
            afflicted = defender.Combat.LifePoints.Max;
        }

        return new DamageResult(afflicted, rawDamage, rawProtection, damage, protection, isCritical);
    }
}
