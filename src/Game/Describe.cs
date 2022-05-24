using System;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Game;

public static class Describe // Helper for converting enums to game text
{
    public static TextId DescribePlayerClass(PlayerClass playerClass)
        => playerClass switch
        {
            PlayerClass.Pilot => Base.SystemText.Class_Pilot,
            PlayerClass.Scientist => Base.SystemText.Class_Scientist,
            PlayerClass.IskaiWarrior => Base.SystemText.Class_Warrior2,
            PlayerClass.DjiKasMage => Base.SystemText.Class_DjiKasMage,
            PlayerClass.Druid => Base.SystemText.Class_Druid,
            PlayerClass.EnlightenedOne => Base.SystemText.Class_EnlightenedOne,
            PlayerClass.Technician => Base.SystemText.Class_Technician,
            PlayerClass.OquloKamulos => Base.SystemText.Class_OquloKamulos,
            PlayerClass.Warrior => Base.SystemText.Class_Warrior,
            _ => throw new ArgumentOutOfRangeException(nameof(playerClass), playerClass, $"Unhandled item type \"{playerClass}\"")
        };

    public static TextId DescribeItemType(ItemType type)
        => type switch
        {
            ItemType.Armor => Base.SystemText.ItemType_Armor,
            ItemType.Helmet => Base.SystemText.ItemType_Helmet,
            ItemType.Shoes => Base.SystemText.ItemType_Shoes,
            ItemType.Shield => Base.SystemText.ItemType_Shield,
            ItemType.CloseRangeWeapon => Base.SystemText.ItemType_CloseRangeWeapon,
            ItemType.LongRangeWeapon => Base.SystemText.ItemType_LongRangeWeapon,
            ItemType.Ammo => Base.SystemText.ItemType_Ammo,
            ItemType.Document => Base.SystemText.ItemType_Document,
            ItemType.SpellScroll => Base.SystemText.ItemType_SpellScroll,
            ItemType.Drink => Base.SystemText.ItemType_Drink,
            ItemType.Amulet => Base.SystemText.ItemType_Amulet,
            ItemType.MagicRing => Base.SystemText.ItemType_Ring,
            ItemType.Valuable => Base.SystemText.ItemType_Valuable,
            ItemType.Tool => Base.SystemText.ItemType_Tool,
            ItemType.Key => Base.SystemText.ItemType_Key,
            ItemType.Misc => Base.SystemText.ItemType_Normal,
            ItemType.MagicItem => Base.SystemText.ItemType_MagicalItem,
            ItemType.HeadsUpDisplayItem => Base.SystemText.ItemType_SpecialItem,
            ItemType.Lockpick => Base.SystemText.ItemType_Lockpick,
            ItemType.LightSource => Base.SystemText.ItemType_Torch,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"Unhandled item type \"{type}\"")
        };
}