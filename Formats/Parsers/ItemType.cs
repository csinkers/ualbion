namespace UAlbion.Formats.Parsers
{
    public enum ItemType : byte
    {
        Armor = 1,
        Helmet = 2,
        Shoes = 3,
        Shield = 4,
        CloseRangeWeapon = 5,
        LongRangeWeapon = 6,
        Ammo = 7,
        Document = 8,
        SpellScroll = 9,
        Drink = 10,
        Amulet = 11,
        MagicRing = 13,
        Valuable = 14,
        Tool = 15,
        Key = 16,
        Misc = 17, // Various useless objects
        MagicItem = 18,
        HeadsUpDisplayItem = 19,
        Lockpick = 21,
        LightSource = 22,
    }
}