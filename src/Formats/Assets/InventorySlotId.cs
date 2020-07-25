using System;

namespace UAlbion.Formats.Assets
{
    public struct InventorySlotId : IEquatable<InventorySlotId>
    {
        public InventorySlotId(InventoryType type, ushort id, ItemSlotId slot)
        {
            Type = type;
            Id = id;
            Slot = slot;
        }

        public InventorySlotId(InventoryId id, ItemSlotId slot) : this(id.Type, id.Id, slot) { }
        public override string ToString() => $"{Type}:{Id}:{Slot}";

        public InventoryType Type { get; }
        public ushort Id { get; }
        public ItemSlotId Slot { get; }

        public InventoryId Inventory => new InventoryId(Type, Id);

        public static bool operator ==(InventorySlotId x, InventorySlotId y) => x.Equals(y);
        public static bool operator !=(InventorySlotId x, InventorySlotId y) => !(x == y);
        public bool Equals(InventorySlotId other) => Type == other.Type && Id == other.Id && Slot == other.Slot;
        public override bool Equals(object obj) => obj is InventorySlotId other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Type;
                hashCode = (hashCode * 397) ^ Id.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Slot;
                return hashCode;
            }
        }
    }
}