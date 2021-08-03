using System;
using System.Numerics;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Assets
{
    public class ItemSlot : IReadOnlyItemSlot
    {
        // Note: an amount of ushort.MaxValue is used for merchant slots containing an unlimited supply of an item (marked with a * in the UI)
        public const ushort Unlimited = ushort.MaxValue;
        public const ushort MaxItemCount = 99;

        void CheckAssumptions()
        {
            if (Item == null && Amount != 0)
                ApiUtil.Assert("Item is not set but amount is non-zero");

            if (Item != null && Amount == 0)
                ApiUtil.Assert("Item is set but amount is zero");

            if (Item is ItemData && Amount > MaxItemCount && Amount != Unlimited)
                ApiUtil.Assert($"Amount ({Amount}) is above the limit of {MaxItemCount}x{Item}");

            if (Amount > short.MaxValue && Amount != Unlimited)
                ApiUtil.Assert($"Tried to put more than {short.MaxValue} items in a gold/rations inventory slot");
        }

        IContents _item;
        public ItemSlot(InventorySlotId id) => Id = id;
        [JsonIgnore] public InventorySlotId Id { get; }
        public byte Charges { get; set; }
        public byte Enchantment { get; set; }
        public Vector2 LastUiPosition { get; set; }
        public ItemSlotFlags Flags { get; set; }
        public ushort Amount { get; set; }

        [JsonIgnore]
        public IContents Item
        {
            get => Amount == 0 ? null : _item;
            set
            {
                _item = value;
                if (_item == null)
                {
                    Amount = 0;
                    Charges = 0;
                    Enchantment = 0;
                    Flags = 0;
                }
                else if (Amount == 0)
                    Amount = 1;
            }
        }

        public ItemId ItemId
        {
            get => Item switch
            {
                Gold _ => AssetId.Gold,
                Rations _ => AssetId.Rations,
                ItemData item => item.Id,
                ItemProxy item => item.Id,
                _ => AssetId.None
            };
            set
            {
                _item = value.Type switch
                {
                    AssetType.Gold => new Gold(),
                    AssetType.Rations => new Rations(),
                    AssetType.Item => new ItemProxy(value),
                    _ => null
                };
            }
        }

        public ItemSlot DeepClone() => (ItemSlot)MemberwiseClone();
        public override string ToString() => Amount == 0 ? "Empty" : $"{Amount}x{ItemId} {Flags}";

        public static ItemSlot Serdes(InventorySlotId id, ItemSlot slot, AssetMapping mapping, ISerializer s)  // 6 per slot
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            slot ??= new ItemSlot(id);
            slot.Amount = s.UInt8(nameof(slot.Amount), (byte)(slot.Amount == Unlimited ? 0xff : slot.Amount));
            if (slot.Amount == 0xff)
                slot.Amount = Unlimited;

            slot.Charges = s.UInt8(nameof(slot.Charges), slot.Charges);
            slot.Enchantment = s.UInt8(nameof(slot.Enchantment), slot.Enchantment);
            slot.Flags = s.EnumU8(nameof(slot.Flags), slot.Flags);

            ItemId itemId = slot.ItemId;
            itemId = ItemId.SerdesU16(nameof(ItemId), itemId, AssetType.Item, mapping, s);
            if (slot.Item == null && !itemId.IsNone)
                slot.Item = new ItemProxy(itemId);
            return slot;
        }

        ushort TransferInner(ItemSlot other, ushort? quantity) // Return the number of items transferred
        {
            switch (other.Item)
            {
                case null: return 0;

                case Gold _ when Item is Gold:
                case Rations _ when Item is Rations:
                {
                    ushort amountToTransfer = Math.Min(other.Amount, quantity ?? (ushort)short.MaxValue);
                    amountToTransfer = Math.Min((ushort)(short.MaxValue - Amount), amountToTransfer);
                    Amount += amountToTransfer;
                    other.Amount -= amountToTransfer;
                    return amountToTransfer;
                }

                case ItemData newItem when Item is ItemData item && item.Id == newItem.Id:
                {
                    if (!item.IsStackable)
                    {
                        ApiUtil.Assert($"Tried to combine non-stackable item {item}");
                        return 0;
                    }

                    ushort amountToTransfer = Math.Min(other.Amount, quantity ?? MaxItemCount);
                    if (Amount != Unlimited)
                    {
                        amountToTransfer = Math.Min((ushort) (MaxItemCount - Amount), amountToTransfer);
                        Amount += amountToTransfer;
                    }

                    if (other.Amount != Unlimited)
                        other.Amount -= amountToTransfer;
                    return amountToTransfer;
                }

                case { } when Item == null:
                {
                    Item = other.Item;
                    Charges = other.Charges;
                    Enchantment = other.Enchantment;
                    Flags = other.Flags;
                    ushort max = other.ItemId == AssetId.Gold || other.ItemId == AssetId.Rations 
                        ? (ushort)short.MaxValue 
                        : MaxItemCount;

                    ushort amountToTransfer = Math.Min(other.Amount, quantity ?? max);
                    amountToTransfer = Math.Min((ushort)(max - Amount), amountToTransfer);
                    Amount = amountToTransfer;
                    if(other.Amount != Unlimited)
                        other.Amount -= amountToTransfer;
                    return amountToTransfer;
                }

                default: 
                    ApiUtil.Assert($"Tried to combine different items: {Item} and {other.Item}");
                    return 0;
            }
        }

        public ushort TransferFrom(ItemSlot other, ushort? quantity) // Return the number of items transferred
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (quantity == 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            CheckAssumptions();
            other.CheckAssumptions();

            ushort itemsTransferred = TransferInner(other, quantity);

            CheckAssumptions();
            other.CheckAssumptions();
            return itemsTransferred;
        }

        public void Set(IContents item, ushort amount, ItemSlotFlags flags = 0, byte charges = 0, byte enchantment = 0) // Just for tests
        {
            Item = item;
            Amount = amount;
            Flags = flags;
            Charges = charges;
            Enchantment = enchantment;
        }

        public bool CanCoalesce(ItemSlot y)
        {
            if (y == null) throw new ArgumentNullException(nameof(y));
            if (Item == null || y.Item == null) return true; // Anything can coalesce with nothing
            if (ItemId != y.ItemId) return false; // Can't stack dissimilar items
            if (Item is Gold) return true;
            if (Item is Rations) return true;
            if (!(Item is ItemData xi) || !(y.Item is ItemData _)) return false; // If not gold/rations, then both must be items
            if (Id.Slot.IsBodyPart() || y.Id.Slot.IsBodyPart()) return false; // Can't wield / wear stacks
            return xi.IsStackable;
        }

        public void Swap(ItemSlot other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
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
