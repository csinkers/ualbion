using System;
using UAlbion.Config;

namespace UAlbion.Formats.Assets;

public struct InventorySlotId : IEquatable<InventorySlotId>
{
    public InventorySlotId(AssetId id, ItemSlotId slot)
    {
        Id = new InventoryId(id);
        Slot = slot;
    }

    public InventorySlotId(InventoryId id, ItemSlotId slot)
    {
        Id = id;
        Slot = slot;
    }

    public InventorySlotId(InventoryType type, ushort id, ItemSlotId slot)
    {
        Id = new InventoryId(type, id);
        Slot = slot;
    }

    public override string ToString() => $"{Id}:{Slot}";

    public InventoryId Id { get; }
    public ItemSlotId Slot { get; }

    public static bool operator ==(InventorySlotId x, InventorySlotId y) => x.Equals(y);
    public static bool operator !=(InventorySlotId x, InventorySlotId y) => !(x == y);
    public bool Equals(InventorySlotId other) => Id == other.Id && Slot == other.Slot;
    public override bool Equals(object obj) => obj is InventorySlotId other && Equals(other);
    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (int)Id;
            hashCode = (hashCode * 397) ^ (int)Slot;
            return hashCode;
        }
    }
}