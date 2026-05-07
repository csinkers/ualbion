using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Combat;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;
using UAlbion.Game.State;

namespace UAlbion.Game.Magic;

public class OutOfCombatMagicManager : GameComponent
{
    public OutOfCombatMagicManager() => On<UseOutOfCombatSpellEvent>(OnUseMagic);

    void OnUseMagic(UseOutOfCombatSpellEvent e)
    {
        var caster = Resolve<IParty>()[e.CasterId];
        if (caster == null) return;

        var spellEntries = caster.Effective.Magic.SpellStrengths
            .Where(kvp =>
            {
                var data = Assets.LoadSpell(kvp.Key);
                return data != null &&
                       (data.Environments & SpellEnvironments.Combat) == 0 &&
                       (data.Environments & (SpellEnvironments.Inventory | SpellEnvironments.Indoors |
                                             SpellEnvironments.Outdoors | SpellEnvironments.Dungeon)) != 0;
            })
            .ToList();

        if (spellEntries.Count == 0) return;

        var casterId = e.CasterId;
        Resolve<IDialogManager>().AddDialog(depth => new SelectSpellDialog(
            spellEntries,
            spellId => CastSpell(casterId, spellId),
            depth));
    }

    void CastSpell(PartyMemberId casterId, SpellId spellId)
    {
        var party = Resolve<IParty>();
        var caster = party[casterId];
        if (caster == null) return;

        var spell = Assets.LoadSpell(spellId);
        if (spell == null) return;

        int strength = caster.Effective.Magic.SpellStrengths.TryGetValue(spellId, out ushort s) ? s : 5;
        var effectType = SpellEffectMapping.GetEffectType(spellId);

        if ((spell.Targets & (SpellTargets.Party | SpellTargets.DeadParty)) != 0)
        {
            switch (effectType)
            {
                case SpellEffectType.HealHP:
                {
                    int amount = strength * 5;
                    caster.Heal(amount);
                    Raise(new LogEvent(LogLevel.Info,
                        $"[SPELL] {casterId} cast {spellId} outside combat, heal={amount}"));
                    break;
                }
                case SpellEffectType.HealAllHP:
                {
                    int amount = strength * 5;
                    foreach (var member in party.StatusBarOrder.Where(m => !m.IsDead))
                    {
                        member.Heal(amount);
                    }
                    Raise(new LogEvent(LogLevel.Info,
                        $"[SPELL] {casterId} cast {spellId} outside combat, heal all={amount}"));
                    break;
                }
                case SpellEffectType.Recuperation:
                case SpellEffectType.Regeneration:
                {
                    int amount = strength * 5;
                    foreach (var member in party.StatusBarOrder.Where(m => !m.IsDead))
                    {
                        member.Heal(amount);
                        CureCondition(member, PlayerConditions.Poisoned);
                        CureCondition(member, PlayerConditions.Intoxicated);
                        CureCondition(member, PlayerConditions.Ill);
                        if (effectType == SpellEffectType.Regeneration)
                        {
                            CureCondition(member, PlayerConditions.Asleep);
                            CureCondition(member, PlayerConditions.Paralysed);
                            CureCondition(member, PlayerConditions.Blind);
                            CureCondition(member, PlayerConditions.Insane);
                            CureCondition(member, PlayerConditions.Panicking);
                            CureCondition(member, PlayerConditions.Irritated);
                            CureCondition(member, PlayerConditions.Exhausted);
                        }
                    }
                    Raise(new LogEvent(LogLevel.Info,
                        $"[SPELL] {casterId} cast {effectType} {spellId} outside combat, heal+condition cure all"));
                    break;
                }
                case SpellEffectType.CureAllConditions:
                    foreach (var member in party.StatusBarOrder)
                    {
                        CureCondition(member, PlayerConditions.Poisoned);
                        CureCondition(member, PlayerConditions.Intoxicated);
                        CureCondition(member, PlayerConditions.Ill);
                        CureCondition(member, PlayerConditions.Asleep);
                        CureCondition(member, PlayerConditions.Paralysed);
                        CureCondition(member, PlayerConditions.Blind);
                        CureCondition(member, PlayerConditions.Insane);
                        CureCondition(member, PlayerConditions.Panicking);
                        CureCondition(member, PlayerConditions.Irritated);
                        CureCondition(member, PlayerConditions.Exhausted);
                    }
                    Raise(new LogEvent(LogLevel.Info,
                        $"[SPELL] {casterId} cast cure all {spellId} outside combat"));
                    break;
                default:
                {
                    var cured = SpellEffectMapping.GetConditionCured(effectType);
                    if (cured.HasValue)
                    {
                        foreach (var member in party.StatusBarOrder)
                        {
                            CureCondition(member, cured.Value);
                        }
                        Raise(new LogEvent(LogLevel.Info,
                            $"[SPELL] {casterId} cast cure {cured} {spellId} outside combat"));
                    }
                    else
                    {
                        int amount = strength * 5;
                        caster.Heal(amount);
                        Raise(new LogEvent(LogLevel.Info,
                            $"[SPELL] {casterId} cast fallback heal {spellId} outside combat, heal={amount}"));
                    }
                    break;
                }
            }
        }
        // Enemy-targeting spells have no valid targets outside combat — silently ignore
    }

    static void CureCondition(ICombatParticipant target, PlayerConditions condition)
    {
        var existing = target.Effective.Combat.Conditions;
        if ((existing & condition) == 0) return;
        target.ClearCondition(condition);
    }
}
