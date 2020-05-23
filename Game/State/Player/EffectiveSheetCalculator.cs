using UAlbion.Formats.Assets;

namespace UAlbion.Game.State.Player
{
    public static class EffectiveSheetCalculator
    {
        public static IEffectiveCharacterSheet GetEffectiveSheet(IAssetManager assets, CharacterSheet sheet)
        {
            var effective = new EffectiveCharacterSheet(sheet.Inventory.InventoryId)
            {
                // Names
                Name = sheet.Name,
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
                SpriteType = sheet.SpriteType,
                PortraitId = sheet.PortraitId,
                EventSetId = sheet.EventSetId,
                WordSetId = sheet.WordSetId,
                Magic = sheet.Magic.DeepClone(),
                Inventory  = sheet.Inventory.DeepClone(),
                Attributes = sheet.Attributes.DeepClone(),
                Skills = sheet.Skills.DeepClone(),
                Combat = sheet.Combat.DeepClone()
            };

            ApplyWieldedItems(assets, effective);
            CalculateTotalWeight(assets, effective);

            return effective;
        }

        static void CalculateTotalWeight(IAssetManager assets, EffectiveCharacterSheet sheet)
        {
            sheet.TotalWeight = 0;
            foreach (var itemSlot in sheet.Inventory.EnumerateAll())
            {
                if (itemSlot.Id == null)
                    continue;
                var item = assets.LoadItem(itemSlot.Id.Value);
                sheet.TotalWeight += itemSlot.Amount * item.Weight;
            }

            sheet.TotalWeight += sheet.Inventory.Gold * 2;
            sheet.TotalWeight += sheet.Inventory.Rations * 250;
            sheet.MaxWeight = sheet.Attributes.Strength * 1000;
        }

        static void ApplyWieldedItems(IAssetManager assets, EffectiveCharacterSheet sheet)
        {
            int initialDamage = sheet.Combat.Damage;
            int initialProtection = sheet.Combat.Protection;

            foreach (var itemSlot in sheet.Inventory.EnumerateBodyParts())
            {
                if (itemSlot.Id == null)
                    continue;
                var item = assets.LoadItem(itemSlot.Id.Value);
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
