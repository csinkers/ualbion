using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;
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

        if ((spell.Targets & (SpellTargets.Party | SpellTargets.DeadParty)) != 0)
        {
            // DEVIATION: Heals caster only; original game showed party member target selection
            int amount = strength * 5;
            caster.Heal(amount);
            Raise(new LogEvent(LogLevel.Info,
                $"[SPELL] {casterId} cast {spellId} outside combat, heal={amount}"));
        }
        // Enemy-targeting spells have no valid targets outside combat — silently ignore
    }
}
