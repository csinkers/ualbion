using System.Numerics;

namespace UAlbion.Formats.Assets
{
    public interface IReadOnlyItemSlot
    {
        ushort Amount { get; }
        byte Charges { get; }
        byte Enchantment { get; }
        ItemSlotFlags Flags { get; }
        IContents Item { get; }
        Vector2 LastUiPosition { get; }
    }
}
