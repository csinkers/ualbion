using System.Numerics;

namespace UAlbion.Game.Gui.Inventory
{
    class InventorySlot : UiElement
    {
        // 70 * 128, 4 * 6

        // Tiles are 16x20 => 64x120
        // 1 pix grid between and around, double thick on right and bottom
        // 1 + 16 + 1 + 16 + 1 + 16 + 1 + 16 + 2
        // = 4*1 + 4*16 + 2 = 70
        // Height = 6 * (1+20) + 2 = 128

        // Button Frame @ (0,0): (70,128) border 1
        // Item0: (0,0):(16,20) border 1
        // Item1: (16,0):(16,20) border 1
        // Item5: (0,20):(16,20) border 1
        // ItemIJ: (16i, 20j):(16,20) border 1

        public InventorySlot(int order) { }

        public override Vector2 GetSize() => new Vector2(16, 20);

        // Func<Item> _itemGetter;
    }
}