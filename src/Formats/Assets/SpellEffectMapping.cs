using System.Collections.Generic;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

/// <summary>
/// Maps spell IDs to their effect types. The original Albion spell data (5 bytes per spell)
/// does not include an effect type field — each spell had a hardcoded handler in the C source
/// (e.g. C1_Spell_4_handler for Blink). This mapping reconstructs that relationship.
///
/// Sources: Horneman MS-DOS source (COMMAGIC.H, COMMAG0-4.C), Walkthrough.txt Magic List,
/// and FreeAlbion reference.
///
/// Spell ID formula: ID = school * 30 + offset + 1
/// School 0 = DjiKas, 1 = DjiKantos, 2 = DjiSchikar (Iskai), 3 = DjiOblida, 4 = DjiKura, 5 = DjiMadis
/// </summary>
public static class SpellEffectMapping
{
    static readonly Dictionary<SpellId, SpellEffectType> _effectMap = new()
    {
        // ── Class 2: Iskai (DjiSchikar) — Nature/Utility ──
        // Blinding Spark / Blinding Ray / Blinding Storm (set Blind)
        { new SpellId(61), SpellEffectType.SetBlind },
        { new SpellId(62), SpellEffectType.SetBlind },
        { new SpellId(63), SpellEffectType.SetBlind },
        // Frost Splinter / Frost Crystal / Frost Avalanche (freeze + damage)
        { new SpellId(64), SpellEffectType.FrostDamage },
        { new SpellId(65), SpellEffectType.FrostDamage },
        { new SpellId(66), SpellEffectType.FrostDamage },
        // Fungification (damage)
        { new SpellId(67), SpellEffectType.Damage },
        // Hurry (double attacks — buff)
        { new SpellId(68), SpellEffectType.Unknown },
        // Heal Blindness / Heal Intoxication / Heal Poisoning
        { new SpellId(69), SpellEffectType.CureBlind },
        { new SpellId(70), SpellEffectType.CureIntoxicated },
        { new SpellId(71), SpellEffectType.CurePoisoned },
        // Light (dungeon illumination)
        { new SpellId(72), SpellEffectType.Unknown },
        // Light Healing
        { new SpellId(73), SpellEffectType.HealHP },
        // Remove Trap
        { new SpellId(74), SpellEffectType.Unknown },
        // Sleep Spores
        { new SpellId(75), SpellEffectType.Unknown },
        // Thorn Trap / Thorn Snare
        { new SpellId(76), SpellEffectType.Damage },
        { new SpellId(77), SpellEffectType.Unknown },
        // View Of Life
        { new SpellId(78), SpellEffectType.Unknown },

        // ── Class 1: DjiKantos — Healing/Protection ──
        // Goddess's Wrath
        { new SpellId(31), SpellEffectType.Damage },
        // Healing
        { new SpellId(32), SpellEffectType.HealHP },
        // Irritation (prevent spell casting)
        { new SpellId(33), SpellEffectType.Irritate },
        // Lifebringer (recovers everyone's LP)
        { new SpellId(34), SpellEffectType.HealAllHP },
        // Map View
        { new SpellId(35), SpellEffectType.Unknown },
        // Quick Withdrawal
        { new SpellId(36), SpellEffectType.Unknown },
        // Recuperation (LP & SP recovered, rest without sleeping)
        { new SpellId(37), SpellEffectType.Recuperation },
        // Regeneration (heals LP & conditions)
        { new SpellId(38), SpellEffectType.Regeneration },
        // Teleport
        { new SpellId(39), SpellEffectType.Blink },
        // Cure Poisoned
        { new SpellId(40), SpellEffectType.CurePoisoned },
        // Cure Intoxicated (Heile Rausch)
        { new SpellId(41), SpellEffectType.CureIntoxicated },
        // Cure Blind
        { new SpellId(42), SpellEffectType.CureBlind },
        // Cure Asleep
        { new SpellId(43), SpellEffectType.CureAsleep },
        // Cure Paralysed
        { new SpellId(44), SpellEffectType.CureParalysed },
        // Cure Insane
        { new SpellId(45), SpellEffectType.CureInsane },

        // ── Class 3: DjiOblida — Mental ──
        // Boasting / Panic / Shock (fear effects)
        { new SpellId(91), SpellEffectType.Unknown },
        { new SpellId(92), SpellEffectType.Unknown },
        { new SpellId(93), SpellEffectType.Unknown },
        // Irritate
        { new SpellId(94), SpellEffectType.Irritate },
        // Berserk
        { new SpellId(95), SpellEffectType.Unknown },
        // Healing
        { new SpellId(96), SpellEffectType.HealHP },
        // Magic Shield
        { new SpellId(97), SpellEffectType.Unknown },
        // Panic (row)
        { new SpellId(98), SpellEffectType.Unknown },
        // Shock (all)
        { new SpellId(99), SpellEffectType.Unknown },
        // Small Fireball
        { new SpellId(100), SpellEffectType.Damage },
        // Banish Demon / Banish Demons
        { new SpellId(101), SpellEffectType.Unknown },
        { new SpellId(102), SpellEffectType.Unknown },
        // Demon Exodus
        { new SpellId(103), SpellEffectType.Unknown },
        // Cure All (DjiOblida version)
        { new SpellId(104), SpellEffectType.CureAllConditions },
        // Cure Insane (DjiOblida version)
        { new SpellId(105), SpellEffectType.CureInsane },

        // ── Class 4: DjiKura — Dark/Death ──
        // Fireball / Fire Rain / Fire Hail
        { new SpellId(121), SpellEffectType.Damage },
        { new SpellId(122), SpellEffectType.Damage },
        { new SpellId(123), SpellEffectType.Damage },
        // Lightning Strike / Thunderbolt / Thunderstorm
        { new SpellId(124), SpellEffectType.Damage },
        { new SpellId(125), SpellEffectType.Damage },
        { new SpellId(126), SpellEffectType.Damage },
        // Lightning Mine / Lightning Trap / Big variants
        { new SpellId(127), SpellEffectType.Damage },
        { new SpellId(128), SpellEffectType.Damage },
        { new SpellId(129), SpellEffectType.Damage },
        { new SpellId(130), SpellEffectType.Damage },
        // Steal Life / Steal Magic
        { new SpellId(131), SpellEffectType.StealLife },
        { new SpellId(132), SpellEffectType.StealMagic },
        // Personal Protection
        { new SpellId(133), SpellEffectType.Unknown },
        // Remove Trap
        { new SpellId(134), SpellEffectType.Unknown },
        // Kamulos's Gaze (instant death)
        { new SpellId(135), SpellEffectType.Unknown },

        // ── Class 5: DjiMadis — Magic/Defense ──
        { new SpellId(151), SpellEffectType.Unknown },
        { new SpellId(152), SpellEffectType.Unknown },
        { new SpellId(153), SpellEffectType.Unknown },
        { new SpellId(154), SpellEffectType.Unknown },
        { new SpellId(155), SpellEffectType.Unknown },
    };

    static readonly Dictionary<SpellEffectType, PlayerConditions> _conditionCureMap = new()
    {
        { SpellEffectType.CureIntoxicated, PlayerConditions.Intoxicated },
        { SpellEffectType.CurePoisoned, PlayerConditions.Poisoned },
        { SpellEffectType.CureDiseased, PlayerConditions.Ill },
        { SpellEffectType.CureAsleep, PlayerConditions.Asleep },
        { SpellEffectType.CureParalysed, PlayerConditions.Paralysed },
        { SpellEffectType.CureBlind, PlayerConditions.Blind },
        { SpellEffectType.CureInsane, PlayerConditions.Insane },
        { SpellEffectType.CurePanicking, PlayerConditions.Panicking },
        { SpellEffectType.CureIrritated, PlayerConditions.Irritated },
        { SpellEffectType.CureExhausted, PlayerConditions.Exhausted },
    };

    public static SpellEffectType GetEffectType(SpellId spellId)
    {
        return _effectMap.TryGetValue(spellId, out var effect) ? effect : SpellEffectType.Unknown;
    }

    public static PlayerConditions? GetConditionCured(SpellEffectType effect)
    {
        return _conditionCureMap.TryGetValue(effect, out var condition) ? condition : null;
    }

    public static bool IsHealingSpell(SpellId spellId)
    {
        var effect = GetEffectType(spellId);
        return effect == SpellEffectType.HealHP ||
               effect == SpellEffectType.HealAllHP ||
               effect == SpellEffectType.Recuperation ||
               effect == SpellEffectType.Regeneration ||
               effect == SpellEffectType.CureAllConditions ||
               _conditionCureMap.ContainsKey(effect);
    }
}
