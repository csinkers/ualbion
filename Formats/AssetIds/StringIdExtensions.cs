using System;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.AssetIds
{
    public static class StringIdExtensions
    {
        public static StringId ToId(this SystemTextId systemTextId)
            => new StringId(AssetType.SystemText, 0, (int)systemTextId);

        public static StringId ToId(this UAlbionStringId id)
            => new StringId(AssetType.UAlbionText, (int)id, 0);

        public static StringId ToId(this WordId id)
            => new StringId(AssetType.Dictionary, (int)id / 500, (int)id);

        public static StringId ToId(this ItemId id)
            => new StringId(AssetType.ItemNames, (int)id, 0);

        public static StringId ToId(this ItemType type)
            => new StringId(AssetType.SystemText, 0, (int)(type switch
            {
                ItemType.Armor => SystemTextId.ItemType_Armor,
                ItemType.Helmet => SystemTextId.ItemType_Helmet,
                ItemType.Shoes => SystemTextId.ItemType_Shoes,
                ItemType.Shield => SystemTextId.ItemType_Shield,
                ItemType.CloseRangeWeapon => SystemTextId.ItemType_CloseRangeWeapon,
                ItemType.LongRangeWeapon => SystemTextId.ItemType_LongRangeWeapon,
                ItemType.Ammo => SystemTextId.ItemType_Ammo,
                ItemType.Document => SystemTextId.ItemType_Document,
                ItemType.SpellScroll => SystemTextId.ItemType_SpellScroll,
                ItemType.Drink => SystemTextId.ItemType_Drink,
                ItemType.Amulet => SystemTextId.ItemType_Amulet,
                ItemType.MagicRing => SystemTextId.ItemType_Ring,
                ItemType.Valuable => SystemTextId.ItemType_Valuable,
                ItemType.Tool => SystemTextId.ItemType_Tool,
                ItemType.Key => SystemTextId.ItemType_Key,
                ItemType.Misc => SystemTextId.ItemType_Normal,
                ItemType.MagicItem => SystemTextId.ItemType_MagicalItem,
                ItemType.HeadsUpDisplayItem => SystemTextId.ItemType_SpecialItem,
                ItemType.Lockpick => SystemTextId.ItemType_Lockpick,
                ItemType.LightSource => SystemTextId.ItemType_Torch,
                _ => throw new ArgumentOutOfRangeException()
            }));

        public static StringId ToId(this PlayerClass x)
            => new StringId(AssetType.SystemText, 0, (int)(x switch
            {
                PlayerClass.Pilot => SystemTextId.Class_Pilot,
                PlayerClass.Scientist => SystemTextId.Class_Scientist,
                PlayerClass.IskaiWarrior => SystemTextId.Class_Warrior2,
                PlayerClass.DjiKasMage => SystemTextId.Class_DjiKasMage,
                PlayerClass.Druid => SystemTextId.Class_Druid,
                PlayerClass.EnlightenedOne => SystemTextId.Class_EnlightenedOne,
                PlayerClass.Technician => SystemTextId.Class_Technician,
                PlayerClass.OquloKamulos => SystemTextId.Class_OquloKamulos,
                PlayerClass.Warrior => SystemTextId.Class_Warrior,
                _ => throw new ArgumentOutOfRangeException()
            }));
    }
}
