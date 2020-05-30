﻿using UAlbion.Formats.Assets;

namespace UAlbion.Game.State.Player
{
    public static class EffectiveSheetCalculator
    {
        const int GramsPerGold = 2;
        const int GramsPerRation = 250;
        const int CarryWeightPerStrength = 1000;
        public static IEffectiveCharacterSheet GetEffectiveSheet(CharacterSheet sheet)
        {
            var effective = new EffectiveCharacterSheet(sheet.Key)
            {
                // Names
                EnglishName = sheet.EnglishName,
                GermanName = sheet.GermanName,
                FrenchName = sheet.FrenchName,

                // Basic stats
                Type = sheet.Type,
                Gender = sheet.Gender,
                Race = sheet.Race,
                Class = sheet.Class,
                Age = sheet.Age,
                Level = sheet.Level,

                // Display and behaviour
                Languages = sheet.Languages,
                SpriteId = sheet.SpriteId,
                PortraitId = sheet.PortraitId,
                EventSetId = sheet.EventSetId,
                WordSetId = sheet.WordSetId,
                Magic = sheet.Magic.DeepClone(),
                Inventory  = sheet.Inventory?.DeepClone(),
                Attributes = sheet.Attributes.DeepClone(),
                Skills = sheet.Skills.DeepClone(),
                Combat = sheet.Combat.DeepClone()
            };

            ApplyWieldedItems(effective);
            CalculateTotalWeight(effective);

            return effective;
        }

        static void CalculateTotalWeight(EffectiveCharacterSheet sheet)
        {
            sheet.TotalWeight = 0;
            foreach (var itemSlot in sheet.Inventory.EnumerateAll())
            {
                if (!(itemSlot.Item is ItemData item))
                    continue;
                sheet.TotalWeight += itemSlot.Amount * item.Weight;
            }

            sheet.TotalWeight += sheet.Inventory.Gold.Amount * GramsPerGold;
            sheet.TotalWeight += sheet.Inventory.Rations.Amount * GramsPerRation;
            sheet.MaxWeight = sheet.Attributes.Strength * CarryWeightPerStrength;
        }

        static void ApplyWieldedItems(EffectiveCharacterSheet sheet)
        {
            int initialDamage = sheet.Combat.Damage;
            int initialProtection = sheet.Combat.Protection;

            foreach (var itemSlot in sheet.Inventory.EnumerateBodyParts())
            {
                if (!(itemSlot.Item is ItemData item))
                    continue;

                sheet.Combat.Damage += item.Damage;
                sheet.Combat.Protection += item.Protection;
                sheet.Combat.LifePointsMax += item.LpMaxBonus;
                sheet.Magic.SpellPointsMax += item.SpMaxBonus;

                if (item.AttributeBonus != 0)
                {
                    switch (item.AttributeType)
                    {
                        case Attribute.Strength: sheet.Attributes.Strength += item.AttributeBonus; break;
                        case Attribute.Intelligence: sheet.Attributes.Intelligence += item.AttributeBonus; break;
                        case Attribute.Dexterity: sheet.Attributes.Dexterity += item.AttributeBonus; break;
                        case Attribute.Speed: sheet.Attributes.Speed += item.AttributeBonus; break;
                        case Attribute.Stamina: sheet.Attributes.Stamina += item.AttributeBonus; break;
                        case Attribute.Luck: sheet.Attributes.Luck += item.AttributeBonus; break;
                        case Attribute.MagicResistance: sheet.Attributes.MagicResistance += item.AttributeBonus; break;
                        case Attribute.MagicTalent: sheet.Attributes.MagicTalent += item.AttributeBonus; break;
                    }
                }

                if (item.SkillBonus != 0)
                {
                    switch (item.SkillType)
                    {
                        case Skill.Melee: sheet.Skills.CloseCombat += item.SkillBonus; break;
                        case Skill.Ranged: sheet.Skills.RangedCombat += item.SkillBonus; break;
                        case Skill.CriticalChance: sheet.Skills.CriticalChance += item.SkillBonus; break;
                        case Skill.LockPicking: sheet.Skills.LockPicking += item.SkillBonus; break;
                    }
                }

                if(item.SkillTax1 != 0)
                {
                    switch (item.SkillTax1Type)
                    {
                        case Skill.Melee: sheet.Skills.CloseCombat -= item.SkillTax1; break;
                        case Skill.Ranged: sheet.Skills.RangedCombat -= item.SkillTax1; break;
                        case Skill.CriticalChance: sheet.Skills.CriticalChance -= item.SkillTax1; break;
                        case Skill.LockPicking: sheet.Skills.LockPicking -= item.SkillTax1; break;
                    }
                }

                if (item.SkillTax2 != 0)
                {
                    switch (item.SkillTax2Type)
                    {
                        case Skill.Melee: sheet.Skills.CloseCombat -= item.SkillTax2; break;
                        case Skill.Ranged: sheet.Skills.RangedCombat -= item.SkillTax2; break;
                        case Skill.CriticalChance: sheet.Skills.CriticalChance -= item.SkillTax2; break;
                        case Skill.LockPicking: sheet.Skills.LockPicking -= item.SkillTax2; break;
                    }
                }
            }

            sheet.DisplayDamage = sheet.Combat.Damage - initialDamage;
            sheet.DisplayProtection = sheet.Combat.Protection - initialProtection;
        }
    }
}
