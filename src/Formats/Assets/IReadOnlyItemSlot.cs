using System.Numerics;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.Assets;

public interface IReadOnlyItemSlot
{
    ItemId Item { get; }
    ushort Amount { get; }
    ItemSlotFlags Flags { get; }
    byte Charges { get; }
    byte Enchantment { get; }
    Vector2 LastUiPosition { get; }
}