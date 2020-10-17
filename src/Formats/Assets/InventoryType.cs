namespace UAlbion.Formats.Assets
{
    public enum InventoryType : byte
    {
        Player,
        Chest,
        Merchant,
        CombatLoot,
        Temporary // Note: the id and slotId of temporary slots are ignored, and references to the slots can only be obtained via direct reference.
    }
}
