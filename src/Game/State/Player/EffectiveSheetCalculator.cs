using System;
using UAlbion.Api.Settings;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using InvVars = UAlbion.Formats.Config.GameVars.Inventory;

namespace UAlbion.Game.State.Player;

public static class EffectiveSheetCalculator
{
    public static IEffectiveCharacterSheet GetEffectiveSheet(
        CharacterSheet sheet,
        IVarSet config,
        Func<ItemId, ItemData> getItem)
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
            Inventory = sheet.Inventory?.DeepClone(),
            Attributes = sheet.Attributes.DeepClone(),
            Skills = sheet.Skills.DeepClone(),
            Combat = sheet.Combat.DeepClone()
        };

        ApplyWieldedItems(effective, getItem);
        CalculateTotalWeight(effective, config, getItem);

        return effective;
    }

    static void CalculateTotalWeight(EffectiveCharacterSheet sheet, IVarSet config, Func<ItemId, ItemData> getItem)
    {
        sheet.TotalWeight = 0;
        foreach (var itemSlot in sheet.Inventory.EnumerateAll())
        {
            if (itemSlot.Item.Type != AssetType.Item)
                continue;

            var item = getItem(itemSlot.Item);
            sheet.TotalWeight += itemSlot.Amount * item.Weight;
        }

        sheet.TotalWeight += (sheet.Inventory.Gold.Amount * InvVars.GramsPerGold.Read(config)) / 10;
        sheet.TotalWeight += sheet.Inventory.Rations.Amount * InvVars.GramsPerRation.Read(config);
        sheet.MaxWeight = sheet.Attributes.Strength.Current * InvVars.CarryWeightPerStrength.Read(config);
    }

    static void ApplyWieldedItems(EffectiveCharacterSheet sheet, Func<ItemId, ItemData> getItem)
    {
        int initialDamage = sheet.Combat.UnknownD8;
        int initialProtection = sheet.Combat.UnknownD6;

        foreach (var itemSlot in sheet.Inventory.EnumerateBodyParts())
        {
            if (itemSlot.Item.Type != AssetType.Item)
                continue;

            var item = getItem(itemSlot.Item);
            sheet.Combat.UnknownD8 += item.Damage;
            sheet.Combat.UnknownD6 += item.Protection;
            sheet.Combat.LifePoints.Max += item.LpMaxBonus;
            sheet.Magic.SpellPoints.Max += item.SpMaxBonus;

            if (item.AttributeBonus != 0)
            {
                switch (item.AttributeType)
                {
                    case PhysicalAttribute.Strength: sheet.Attributes.Strength.Current += item.AttributeBonus; break;
                    case PhysicalAttribute.Intelligence: sheet.Attributes.Intelligence.Current += item.AttributeBonus; break;
                    case PhysicalAttribute.Dexterity: sheet.Attributes.Dexterity.Current += item.AttributeBonus; break;
                    case PhysicalAttribute.Speed: sheet.Attributes.Speed.Current += item.AttributeBonus; break;
                    case PhysicalAttribute.Stamina: sheet.Attributes.Stamina.Current += item.AttributeBonus; break;
                    case PhysicalAttribute.Luck: sheet.Attributes.Luck.Current += item.AttributeBonus; break;
                    case PhysicalAttribute.MagicResistance: sheet.Attributes.MagicResistance.Current += item.AttributeBonus; break;
                    case PhysicalAttribute.MagicTalent: sheet.Attributes.MagicTalent.Current += item.AttributeBonus; break;
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

            if (item.SkillTax1 != 0)
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