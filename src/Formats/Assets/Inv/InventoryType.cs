namespace UAlbion.Formats.Assets.Inv;

public enum InventoryType : byte
{
    Unknown,
    Player,
    Chest,
    Merchant,
    Monster,
    CombatLoot,
    Temporary // Note: the id and slotId of temporary slots are ignored, and references to the slots can only be obtained via direct reference.
}