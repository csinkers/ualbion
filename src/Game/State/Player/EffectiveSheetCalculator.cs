using System;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Config;
using Attribute = UAlbion.Formats.Assets.Attribute;

namespace UAlbion.Game.State.Player;

public static class EffectiveSheetCalculator
{
    public static IEffectiveCharacterSheet GetEffectiveSheet(CharacterSheet sheet, GameConfig config)
    {
        if (sheet == null) throw new ArgumentNullException(nameof(sheet));
        if (config == null) throw new ArgumentNullException(nameof(config));
        var effective = new EffectiveCharacterSheet(sheet.Id)
        {
            // Names
            EnglishName = sheet.EnglishName,
            GermanName = sheet.GermanName,
            FrenchName = sheet.FrenchName,

            // Basic stats
            Type = sheet.Type,
            Gender = sheet.Gender,
            Race = sheet.Race,
            PlayerClass = sheet.PlayerClass,
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
        CalculateTotalWeight(effective, config);

        return effective;
    }

    static void CalculateTotalWeight(EffectiveCharacterSheet sheet, GameConfig config)
    {
        sheet.TotalWeight = 0;
        foreach (var itemSlot in sheet.Inventory.EnumerateAll())
        {
            if (itemSlot.Item is not ItemData item)
                continue;
            sheet.TotalWeight += itemSlot.Amount * item.Weight;
        }

        sheet.TotalWeight += (sheet.Inventory.Gold.Amount * config.Inventory.GramsPerGold) / 10;
        sheet.TotalWeight += sheet.Inventory.Rations.Amount * config.Inventory.GramsPerRation;
        sheet.MaxWeight = sheet.Attributes.Strength.Current * config.Inventory.CarryWeightPerStrength;
    }

    static void ApplyWieldedItems(EffectiveCharacterSheet sheet)
    {
        int initialDamage = sheet.Combat.UnknownD8;
        int initialProtection = sheet.Combat.UnknownD6;

        foreach (var itemSlot in sheet.Inventory.EnumerateBodyParts())
        {
            if (itemSlot.Item is not ItemData item)
                continue;

            sheet.Combat.UnknownD8 += item.Damage;
            sheet.Combat.UnknownD6 += item.Protection;
            sheet.Combat.LifePoints.Max += item.LpMaxBonus;
            sheet.Magic.SpellPoints.Max += item.SpMaxBonus;

            if (item.AttributeBonus != 0)
            {
                switch (item.AttributeType)
                {
                    case Attribute.Strength: sheet.Attributes.Strength.Current += item.AttributeBonus; break;
                    case Attribute.Intelligence: sheet.Attributes.Intelligence.Current += item.AttributeBonus; break;
                    case Attribute.Dexterity: sheet.Attributes.Dexterity.Current += item.AttributeBonus; break;
                    case Attribute.Speed: sheet.Attributes.Speed.Current += item.AttributeBonus; break;
                    case Attribute.Stamina: sheet.Attributes.Stamina.Current += item.AttributeBonus; break;
                    case Attribute.Luck: sheet.Attributes.Luck.Current += item.AttributeBonus; break;
                    case Attribute.MagicResistance: sheet.Attributes.MagicResistance.Current += item.AttributeBonus; break;
                    case Attribute.MagicTalent: sheet.Attributes.MagicTalent.Current += item.AttributeBonus; break;
                }
            }

            if (item.SkillBonus != 0)
            {
                switch (item.SkillType)
                {
                    case Skill.Melee: sheet.Skills.CloseCombat.Current += item.SkillBonus; break;
                    case Skill.Ranged: sheet.Skills.RangedCombat.Current += item.SkillBonus; break;
                    case Skill.CriticalChance: sheet.Skills.CriticalChance.Current += item.SkillBonus; break;
                    case Skill.LockPicking: sheet.Skills.LockPicking.Current += item.SkillBonus; break;
                }
            }

            if(item.SkillTax1 != 0)
            {
                switch (item.SkillTax1Type)
                {
                    case Skill.Melee: sheet.Skills.CloseCombat.Current -= item.SkillTax1; break;
                    case Skill.Ranged: sheet.Skills.RangedCombat.Current -= item.SkillTax1; break;
                    case Skill.CriticalChance: sheet.Skills.CriticalChance.Current -= item.SkillTax1; break;
                    case Skill.LockPicking: sheet.Skills.LockPicking.Current -= item.SkillTax1; break;
                }
            }

            if (item.SkillTax2 != 0)
            {
                switch (item.SkillTax2Type)
                {
                    case Skill.Melee: sheet.Skills.CloseCombat.Current -= item.SkillTax2; break;
                    case Skill.Ranged: sheet.Skills.RangedCombat.Current -= item.SkillTax2; break;
                    case Skill.CriticalChance: sheet.Skills.CriticalChance.Current -= item.SkillTax2; break;
                    case Skill.LockPicking: sheet.Skills.LockPicking.Current -= item.SkillTax2; break;
                }
            }
        }

        sheet.DisplayDamage = sheet.Combat.UnknownD8 - initialDamage;
        sheet.DisplayProtection = sheet.Combat.UnknownD6 - initialProtection;
    }
}