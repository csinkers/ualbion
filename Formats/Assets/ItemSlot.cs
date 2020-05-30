using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets
{
    public class ItemSlot : IReadOnlyItemSlot
    {
        ushort _amount;

        public ItemSlot(InventorySlotId id) => Id = id;

        public InventorySlotId Id { get; }
        public ushort Amount
        {
            get => _amount;
            set
            {
                if (Item is ItemData && value > byte.MaxValue)
                {
                    ApiUtil.Assert($"Tried to put more than {byte.MaxValue} items in an inventory slot");
                    _amount = byte.MaxValue;
                }
                else if (value > short.MaxValue)
                {
                    ApiUtil.Assert($"Tried to put more than {byte.MaxValue} items in an inventory slot");
                    _amount = (ushort)short.MaxValue;
                }
                else _amount = value;
            }
        }

        public byte Charges { get; set; }
        public byte Enchantment { get; set; }
        public ItemSlotFlags Flags { get; set; }
        public IContents Item { get; set; }

        public ItemSlot DeepClone() => (ItemSlot)MemberwiseClone();
        public override string ToString() => Amount == 0 ? "Empty" : $"{Amount}x{Item} {Flags}";

        public static ItemSlot Serdes(InventorySlotId id, ItemSlot slot, ISerializer s)  // 6 per slot
        {
            slot ??= new ItemSlot(id) { Amount = 0, Item = null };
            slot.Amount = s.UInt8(nameof(slot.Amount), (byte)slot.Amount);
            slot.Charges = s.UInt8(nameof(slot.Charges), slot.Charges);
            slot.Enchantment = s.UInt8(nameof(slot.Enchantment), slot.Enchantment);
            slot.Flags = s.EnumU8(nameof(slot.Flags), slot.Flags);

            ItemId? itemId = (slot.Item as IItem)?.Id;
            itemId = (ItemId?)StoreIncrementedNullZero.Serdes(nameof(itemId), (ushort?)itemId, s.UInt16);
            if(slot.Item == null && itemId != null)
                slot.Item = new ItemProxy(itemId.Value);
            return slot;
        }

        public void TransferFrom(ItemSlot other, ushort? quantity)
        {
            if (other == null)
            {
                ApiUtil.Assert("Tried to transfer from non-existant slot");
                return;
            }

            switch (other.Item)
            {
                case null:
                    if(Item != null)
                        ApiUtil.Assert($"Item destroyed! ({Amount}x{Item})");
                    Item = null;
                    Amount = 0;
                    Charges = 0;
                    Enchantment = 0;
                    Flags = 0;
                    break;

                case Gold _ when Item is Gold:
                case Rations _ when Item is Rations:
                {
                    ushort amountToTransfer = Math.Min(other.Amount, quantity ?? (ushort)(short.MaxValue));
                    amountToTransfer = Math.Min((ushort)(short.MaxValue - Amount), amountToTransfer);
                    Amount += amountToTransfer;
                    other.Amount -= amountToTransfer;
                    break;
                }

                case ItemData newItem when Item is ItemData item && item.Id == newItem.Id:
                {
                    if (!item.IsStackable)
                    {
                        ApiUtil.Assert($"Tried to combine non-stackable item {item}");
                        return;
                    }

                    ushort amountToTransfer = Math.Min(other.Amount, quantity ?? byte.MaxValue);
                    amountToTransfer = Math.Min((ushort)(byte.MaxValue - Amount), amountToTransfer);
                    Amount += amountToTransfer;
                    other.Amount -= amountToTransfer;
                    break;
                }

                case { } when Item == null:
                {
                    ushort amountToTransfer = Math.Min(other.Amount, quantity ?? byte.MaxValue);
                    amountToTransfer = Math.Min((ushort)(byte.MaxValue - Amount), amountToTransfer);
                    Amount = amountToTransfer;
                    other.Amount -= amountToTransfer;

                    Item = other.Item;
                    Charges = other.Charges;
                    Enchantment = other.Enchantment;
                    Flags = other.Flags;
                    break;
                }

                default: 
                    ApiUtil.Assert($"Tried to combine different items: {Item} and {other.Item}");
                    break;
            }

            if (other.Amount == 0 && other.Item is ItemData)
                other.Clear();
        }

        public bool CanCoalesce(ItemSlot y)
        {
            if (Item == null || y.Item == null) return true; // Anything can coalesce with nothing
            if (Item is Gold && y.Item is Gold) return true;
            if (Item is Rations && y.Item is Rations) return true;
            if (!(Item is ItemData xi) || !(y.Item is ItemData yi)) return false; // If not gold/rations, then both must be items
            if (xi.Id != yi.Id) return false; // Can't stack dissimilar items
            if (Id.Slot.IsBodyPart() || y.Id.Slot.IsBodyPart()) return false; // Can't wield / wear stacks
            return xi.IsStackable;
        }

        public void Swap(ItemSlot other)
        {
            var oldItem = Item; Item = other.Item; other.Item = oldItem;
            var oldAmount = Amount; Amount = other.Amount; other.Amount = oldAmount;
            var oldCharges = Charges; Charges = other.Charges; other.Charges = oldCharges;
            var oldEnchantment = Enchantment; Enchantment = other.Enchantment; other.Enchantment = oldEnchantment;
            var oldFlags = Flags; Flags = other.Flags; other.Flags = oldFlags;
        }

        public void Clear()
        {
            Item = null;
            Amount = 0;
            Charges = 0;
            Enchantment = 0;
            Flags = 0;
        }
    }
}
