namespace UAlbion.Formats.Parsers
{
    public enum AmmunitionType : byte
    {
        Intrinsic = 0, // used for throwing axes etc, as well as items that aren't ranged weapons
        Arrow = 1,
        Bolt = 2,
        Canister = 3, // i.e. bullets
    }
}