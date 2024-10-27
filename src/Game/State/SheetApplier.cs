using System;
using UAlbion.Api.Eventing;
using UAlbion.Formats;
using UAlbion.Formats.Assets.Inv;
using UAlbion.Formats.Assets.Sheets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;

namespace UAlbion.Game.State;

public class SheetApplier : Component
{
    public void Apply(IDataChangeEvent e, CharacterSheet sheet)
    {
        ArgumentNullException.ThrowIfNull(e);
        ArgumentNullException.ThrowIfNull(sheet);

        switch (e)
        {
            case ChangeEventSetEvent esetEvent: sheet.EventSetId = esetEvent.EventSet; break;
            case ChangeWordSetEvent wordEvent:  sheet.WordSetId  = wordEvent.WordSet; break;
            case ChangeAttributeEvent attribEvent:  ApplyAttribute(sheet, attribEvent); break;
            case ChangeSkillEvent skillEvent:       ApplySkill(sheet, skillEvent); break;
            case ChangeLanguageEvent languageEvent: ApplyLanguage(sheet, languageEvent); break;
            case ChangeStatusEvent statusEvent:     ApplyStatus(sheet, statusEvent); break;
            case ChangeItemEvent itemEvent:         Warn($"TODO: {itemEvent} not handled"); break;
            case ChangeSpellsEvent spellsEvent:     ApplySpells(sheet, spellsEvent); break;
            case DataChangeEvent generic:           ApplyGeneric(sheet, generic); break;
            default: throw new ArgumentOutOfRangeException(nameof(e));
        }

        Raise(new InventoryChangedEvent(new InventoryId(sheet.Id)));
        Raise(new SheetChangedEvent(sheet.Id));
    }

    void ApplyGeneric(CharacterSheet sheet, DataChangeEvent generic)
    {
        var amount = generic.IsRandom
            ? (ushort)Resolve<IRandom>().Generate(generic.Amount)
            : generic.Amount;

        switch (generic.ChangeProperty)
        {
            case ChangeProperty.Health:
                sheet.Combat.LifePoints.Apply(generic.Operation, amount);
                LifeChecks(sheet);
                break;

            case ChangeProperty.Mana:
                sheet.Magic.SpellPoints.Apply(generic.Operation, amount);
                break;

            case ChangeProperty.MaxHealth:
                sheet.Combat.LifePoints.ApplyToMax(generic.Operation, amount);
                LifeChecks(sheet);
                break;

            case ChangeProperty.MaxMana:
                sheet.Magic.SpellPoints.ApplyToMax(generic.Operation, amount);
                break;

            case ChangeProperty.Experience:
                sheet.Combat.ExperiencePoints = generic.Operation.Apply(sheet.Combat.ExperiencePoints, amount);
                // ExperienceChecks(sheet);
                break;

            case ChangeProperty.TrainingPoints:
                sheet.Combat.TrainingPoints = generic.Operation.Apply16(sheet.Combat.TrainingPoints, amount);
                break;
            case ChangeProperty.Gold:
                sheet.Inventory.Gold.Amount = generic.Operation.Apply16(sheet.Inventory.Gold.Amount, amount);
                break;
            case ChangeProperty.Food:
                sheet.Inventory.Rations.Amount = generic.Operation.Apply16(sheet.Inventory.Rations.Amount, amount);
                break;

            case ChangeProperty.Unused4:
            case ChangeProperty.Unused6:
            case ChangeProperty.UnusedA:
            case ChangeProperty.UnusedB:
            case ChangeProperty.UnusedE:
            case ChangeProperty.UnusedF:
                Warn($"TODO: {generic} not handled");
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(generic), $"Event was of unexpected generic type {generic.ChangeProperty}");
        }
    }

    void ApplySpells(CharacterSheet sheet, ChangeSpellsEvent spellsEvent)
    {
        var spellManager = Resolve<ISpellManager>();
        var spellId = spellManager.GetSpellId(spellsEvent.School, (byte)spellsEvent.SpellNumber);
        var known = sheet.Magic.KnownSpells.Contains(spellId);

        switch (known)
        {
            case true when spellsEvent.Operation is NumericOperation.SetToMinimum or NumericOperation.Toggle:
                sheet.Magic.KnownSpells.Remove(spellId);
                break;
            case false when spellsEvent.Operation is NumericOperation.SetToMaximum or NumericOperation.Toggle:
                sheet.Magic.KnownSpells.Add(spellId);
                break;
        }

        // TODO: Verify if this event changes spell strengths, or just known spells
    }

    static void ApplyStatus(CharacterSheet sheet, ChangeStatusEvent statusEvent)
    {
        var condition = statusEvent.Status.ToFlag();
        sheet.Combat.Conditions = statusEvent.Operation switch
        {
            NumericOperation.SetToMaximum => sheet.Combat.Conditions | condition,
            NumericOperation.SetToMinimum => sheet.Combat.Conditions & ~condition,
            NumericOperation.Toggle => sheet.Combat.Conditions ^ condition,
            _ => sheet.Combat.Conditions
        };
    }

    static void ApplyLanguage(CharacterSheet sheet, ChangeLanguageEvent languageEvent)
    {
        var lang = languageEvent.Language.ToFlag();
        sheet.Languages = languageEvent.Operation switch
        {
            NumericOperation.SetToMaximum => sheet.Languages | lang,
            NumericOperation.SetToMinimum => sheet.Languages & ~lang,
            NumericOperation.Toggle => sheet.Languages ^ lang,
            _ => sheet.Languages
        };
    }

    void ApplyAttribute(CharacterSheet sheet, ChangeAttributeEvent attribEvent)
    {
        var attrib = attribEvent.Attribute switch
        {
            PhysicalAttribute.Strength => sheet.Attributes.Strength,
            PhysicalAttribute.Intelligence => sheet.Attributes.Intelligence,
            PhysicalAttribute.Dexterity => sheet.Attributes.Dexterity,
            PhysicalAttribute.Speed => sheet.Attributes.Speed,
            PhysicalAttribute.Stamina => sheet.Attributes.Stamina,
            PhysicalAttribute.Luck => sheet.Attributes.Luck,
            PhysicalAttribute.MagicResistance => sheet.Attributes.MagicResistance,
            PhysicalAttribute.MagicTalent => sheet.Attributes.MagicTalent,
            _ => throw new ArgumentException($"Unknown attribute {attribEvent.Attribute} in event {attribEvent}", nameof(attribEvent))
        };

        var amount = attribEvent.IsRandom
            ? (ushort)Resolve<IRandom>().Generate(attribEvent.Amount)
            : attribEvent.Amount;

        attrib.Apply(attribEvent.Operation, amount);
        Raise(new AttributeChangedEvent(sheet.Id, attribEvent.Attribute));
    }

    void ApplySkill(CharacterSheet sheet, ChangeSkillEvent skillEvent)
    {
        var skill = skillEvent.Skill switch
        {
            Skill.Melee => sheet.Skills.CloseCombat,
            Skill.Ranged => sheet.Skills.RangedCombat,
            Skill.CriticalChance => sheet.Skills.CriticalChance,
            Skill.LockPicking => sheet.Skills.LockPicking,
            _ => throw new ArgumentException($"Unknown skill {skillEvent.Skill} in event {skillEvent}", nameof(skillEvent))
        };

        var amount = skillEvent.IsRandom
            ? (ushort)Resolve<IRandom>().Generate(skillEvent.Amount)
            : skillEvent.Amount;

        skill.Apply(skillEvent.Operation, amount);
        Raise(new SkillChangedEvent(sheet.Id, skillEvent.Skill));
    }

    void LifeChecks(CharacterSheet sheet)
    {
        var lp = sheet.Combat.LifePoints;
        if (lp.Max > lp.Current)
            lp.Current = lp.Max;

        if (lp.Current == 0)
            Raise(new DeathEvent(sheet.Id)); // TODO: Death handling
    }

    // void ExperienceChecks(CharacterSheet sheet)
    // {
    //     // TODO: Handle leveling up.
    // }
}
