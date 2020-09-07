using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UAlbion.Formats.Assets
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ItemType : byte
    {
        Armor              = 1,
        Helmet             = 2,
        Shoes              = 3,
        Shield             = 4,
        CloseRangeWeapon   = 5,
        LongRangeWeapon    = 6,
        Ammo               = 7,  // Stackable
        Document           = 8,
        SpellScroll        = 9,
        Drink              = 10, // Stackable
        Amulet             = 11,
        MagicRing          = 13,
        Valuable           = 14, // Stackable
        Tool               = 15, // Stackable
        Key                = 16,
        Misc               = 17, // Various useless objects, stackable
        MagicItem          = 18,
        HeadsUpDisplayItem = 19,
        Lockpick           = 21, // Stackable
        LightSource        = 22, // Stackable
    }
}
