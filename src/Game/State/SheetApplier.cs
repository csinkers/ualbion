using System;
using UAlbion.Api.Eventing;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using Attribute = UAlbion.Formats.Assets.Attribute;

namespace UAlbion.Game.State;

public class SheetApplier : Component
{
    public void Apply(IDataChangeEvent e, CharacterSheet sheet)
    {
        if (e == null) throw new ArgumentNullException(nameof(e));
        if (sheet == null) throw new ArgumentNullException(nameof(sheet));

        switch (e)
        {
            case ChangeEventSetEvent esetEvent: sheet.EventSetId = esetEvent.EventSet; break;
            case ChangeWordSetEvent wordEvent: sheet.WordSetId = wordEvent.WordSet; break;

            case ChangeAttributeEvent attribEvent:
            {
                var attrib = attribEvent.Attribute switch
                {
                    Attribute.Strength        => sheet.Attributes.Strength,
                    Attribute.Intelligence    => sheet.Attributes.Intelligence,
                    Attribute.Dexterity       => sheet.Attributes.Dexterity,
                    Attribute.Speed           => sheet.Attributes.Speed,
                    Attribute.Stamina         => sheet.Attributes.Stamina,
                    Attribute.Luck            => sheet.Attributes.Luck,
                    Attribute.MagicResistance => sheet.Attributes.MagicResistance,
                    Attribute.MagicTalent     => sheet.Attributes.MagicTalent,
                    _ => throw new ArgumentException(nameof(attribEvent), $"Unknown attribute {attribEvent.Attribute} in event {e}")
                };

                var amount = attribEvent.IsRandom
                    ? (ushort)Resolve<IRandom>().Generate(attribEvent.Amount)
                    : attribEvent.Amount;

                attrib.Apply(e.Operation, amount);
                Raise(new AttributeChangedEvent(sheet.Id, attribEvent.Attribute));
                break;
            }

            case ChangeSkillEvent skillEvent:
            {
                var skill = skillEvent.Skill switch
                {
                    Skill.Melee          => sheet.Skills.CloseCombat,
                    Skill.Ranged         => sheet.Skills.RangedCombat,
                    Skill.CriticalChance => sheet.Skills.CriticalChance,
                    Skill.LockPicking    => sheet.Skills.LockPicking,
                    _ => throw new ArgumentException(nameof(skillEvent), $"Unknown skill {skillEvent.Skill} in event {e}")
                };

                var amount = skillEvent.IsRandom
                    ? (ushort)Resolve<IRandom>().Generate(skillEvent.Amount)
                    : skillEvent.Amount;

                skill.Apply(e.Operation, amount);
                Raise(new SkillChangedEvent(sheet.Id, skillEvent.Skill));
                break;
            }

            case ChangeLanguageEvent languageEvent:
                var lang = languageEvent.Language.ToFlag();
                sheet.Languages = languageEvent.Operation switch
                {
                    NumericOperation.SetToMaximum => sheet.Languages | lang,
                    NumericOperation.SetToMinimum => sheet.Languages & ~lang,
                    NumericOperation.Toggle => sheet.Languages ^ lang,
                    _ => sheet.Languages
                };
                break;

            case ChangeStatusEvent statusEvent:
                var condition = statusEvent.Status.ToFlag();
                sheet.Combat.Conditions = statusEvent.Operation switch
                {
                    NumericOperation.SetToMaximum => sheet.Combat.Conditions | condition,
                    NumericOperation.SetToMinimum => sheet.Combat.Conditions & ~condition,
                    NumericOperation.Toggle => sheet.Combat.Conditions ^ condition,
                    _ => sheet.Combat.Conditions
                };
                break;

            case ChangeItemEvent itemEvent:
                // TODO
                break;

            case ChangeSpellsEvent spellsEvent:
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
                break;

            case DataChangeEvent generic:
            {
                var amount = generic.IsRandom
                    ? (ushort)Resolve<IRandom>().Generate(generic.Amount)
                    : generic.Amount;

                switch (generic.ChangeProperty)
                {
                    case ChangeProperty.Health:
                        sheet.Combat.LifePoints.Apply(e.Operation, amount);
                        LifeChecks(sheet);
                        break;

                    case ChangeProperty.Mana:
                        sheet.Magic.SpellPoints.Apply(e.Operation, amount);
                        break;

                    case ChangeProperty.MaxHealth:
                        sheet.Combat.LifePoints.ApplyToMax(e.Operation, amount);
                        LifeChecks(sheet);
                        break;

                    case ChangeProperty.MaxMana:
                        sheet.Magic.SpellPoints.ApplyToMax(e.Operation, amount);
                        break;

                    case ChangeProperty.Experience:
                        sheet.Combat.ExperiencePoints = e.Operation.Apply(sheet.Combat.ExperiencePoints, amount);
                        ExperienceChecks(sheet);
                        break;

                    case ChangeProperty.TrainingPoints:
                        sheet.Combat.TrainingPoints = e.Operation.Apply16(sheet.Combat.TrainingPoints, amount);
                        break;
                    case ChangeProperty.Gold:
                        sheet.Inventory.Gold.Amount = e.Operation.Apply16(sheet.Inventory.Gold.Amount, amount);
                        break;
                    case ChangeProperty.Food:
                        sheet.Inventory.Rations.Amount = e.Operation.Apply16(sheet.Inventory.Rations.Amount, amount);
                        break;

                    case ChangeProperty.Unused4:
                        break;
                    case ChangeProperty.Unused6:
                        break;
                    case ChangeProperty.UnusedA:
                        break;
                    case ChangeProperty.UnusedB:
                        break;
                    case ChangeProperty.UnusedE:
                        break;
                    case ChangeProperty.UnusedF:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(e));
        }

        Raise(new InventoryChangedEvent(new InventoryId(sheet.Id)));
        Raise(new SheetChangedEvent(sheet.Id));
    }

    void LifeChecks(CharacterSheet sheet)
    {
        var lp = sheet.Combat.LifePoints;
        if (lp.Max > lp.Current)
            lp.Current = lp.Max;

        if (lp.Current == 0)
            Raise(new DeathEvent(sheet.Id)); // TODO: Death handling
    }

    void ExperienceChecks(CharacterSheet sheet)
    {
        // TODO: Handle leveling up.
    }
}
