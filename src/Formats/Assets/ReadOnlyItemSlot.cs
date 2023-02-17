using System.Numerics;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public class ReadOnlyItemSlot : IReadOnlyItemSlot
{
    readonly ItemSlot _slot;
    public ReadOnlyItemSlot(ItemSlot slot) => _slot = slot;
    public ItemId Item => _slot.Item;
    public ushort Amount => _slot.Amount;
    public ItemSlotFlags Flags => _slot.Flags;
    public byte Charges => _slot.Charges;
    public byte Enchantment => _slot.Enchantment;
    public Vector2 LastUiPosition => _slot.LastUiPosition;
}