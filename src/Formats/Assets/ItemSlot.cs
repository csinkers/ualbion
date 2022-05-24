using System;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

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
    [JsonIgnore] public Vector2 LastUiPosition { get; set; }
    public ushort Amount { get; set; }
    public ItemSlotFlags Flags { get; set; }
    public byte Charges { get; set; }
    public byte Enchantment { get; set; }

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
            Gold => AssetId.Gold,
            Rations => AssetId.Rations,
            ItemData item => item.Id,
            ItemProxy item => item.Id,
            _ => AssetId.None
        };
        set
        {
            Item = value.Type switch
            {
                AssetType.Gold => Gold.Instance,
                AssetType.Rations => Rations.Instance,
                AssetType.Item => new ItemProxy(value),
                _ => null
            };
        }
    }

    public ItemSlot DeepClone() => (ItemSlot) MemberwiseClone();

    /* Test cases:
    Empty
    Item.Dagger
    2x Item.Dagger
    Item.Dagger F(Cursed, Broken)
    Item.SerpentStaff C10 E1
    Item.SerpentStaff F(Unk3) C10 E1
    */
    public override string ToString()
    {
        if (Amount == 0) return "Empty";
        var sb = new StringBuilder();
        if (Amount == Unlimited)
        {
            sb.Append("(Inf) ");
        }
        else if (Amount != 1)
        {
            sb.Append(Amount);
            sb.Append("x ");
        }

        sb.Append(ItemId);
        if (Flags != 0)
        {
            sb.Append(" F(");
            sb.Append(Flags);
            sb.Append(")");
        }

        if (Charges != 0)
        {
            sb.Append(" C");
            sb.Append(Charges);
        }

        if (Charges != 0)
        {
            sb.Append(" E");
            sb.Append(Enchantment);
        }

        return sb.ToString();
    }

    public static ItemSlot Parse(string s)
    {
        var slot = new ItemSlot(new InventorySlotId());
        if (string.IsNullOrWhiteSpace(s) || "Empty".Equals(s, StringComparison.InvariantCultureIgnoreCase))
            return slot;

        int index = 0;
        if (s.StartsWith("(Inf) ", true, CultureInfo.InvariantCulture))
        {
            slot.Amount = Unlimited;
            index = "(Inf) ".Length;
        }
        else if (char.IsDigit(s[0]))
        {
            index = s.IndexOf("x ", StringComparison.Ordinal) + 2;
            if (index == -1)
                throw new FormatException($"ItemSlot \"{s}\" began with a digit, but wasn't followed by \"x \"");

            if (!ushort.TryParse(s[..(index - 2)], NumberStyles.None, CultureInfo.InvariantCulture, out var amount) || amount > MaxItemCount)
                throw new FormatException($"Amount in \"{s}\" was outside the allowed range [0..{MaxItemCount}]");
            slot.Amount = amount;
        }
        else slot.Amount = 1;

        int index2 = s.IndexOf(' ', index);
        var itemId = index2 == -1 ? s[index..] : s[index..index2];
        slot.ItemId = ItemId.Parse(itemId);
        if (index2 == -1)
            return slot;

        char mode = ' ';
        for (int i = index2; i < s.Length; i++)
        {
            switch (mode)
            {
                case ' ': mode = s[i]; break;
                case 'F':
                    mode = s[i];
                    if (mode != '(') throw new FormatException($"Expected '(' after 'F' when parsing ItemSlot \"{s}\"");
                    break;
                case '(':
                    index = s.IndexOf(')', i);
                    if (i == -1) throw new FormatException($"Expected ')' after the flag list when parsing ItemSlot \"{s}\"");
                    slot.Flags = (ItemSlotFlags)Enum.Parse(typeof(ItemSlotFlags), s[i..index]);
                    i = index;
                    mode = ' ';
                    break;
                case 'C':
                case 'E':
                    index = s.IndexOf(' ', i);
                    if (index == -1) index = s.Length;
                    var valueString = s[i..index];
                    var value = byte.Parse(valueString, CultureInfo.InvariantCulture);
                    if (mode == 'C') slot.Charges = value;
                    else slot.Enchantment = value;
                    i = index;
                    mode = ' ';
                    break;
                default: throw new FormatException($"Unexpected '{mode}' in ItemSlot \"{s}\"");
            }
        }

        return slot;
    }

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

            case Gold when Item is Gold:
            case Rations when Item is Rations:
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

    public ItemSlot Set(ItemId item, ushort amount, ItemSlotFlags flags = 0, byte charges = 0, byte enchantment = 0) // Just for tests
        => Set(new ItemProxy(item), amount, flags, charges, enchantment);
    public ItemSlot Set(IContents item, ushort amount, ItemSlotFlags flags = 0, byte charges = 0, byte enchantment = 0) // Just for tests
    {
        Item = item;
        Amount = amount;
        Flags = flags;
        Charges = charges;
        Enchantment = enchantment;
        return this;
    }

    public bool CanCoalesce(ItemSlot y)
    {
        if (y == null) throw new ArgumentNullException(nameof(y));
        if (Item == null || y.Item == null) return true; // Anything can coalesce with nothing
        if (ItemId != y.ItemId) return false; // Can't stack dissimilar items
        if (Item is Gold or Rations) return true;
        if (!(Item is ItemData xi) || !(y.Item is ItemData)) return false; // If not gold/rations, then both must be items
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