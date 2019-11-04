using System.Numerics;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Entities;

namespace UAlbion.Game.Gui.Inventory
{
    class InventoryBodyPart : UiElement
    {
        // Inner area 16x16 w/ 1-pixel button frame
        public InventoryBodyPart(ItemSlotId itemSlotId)
        {
            Children.Add(new ButtonFrame(new FixedSize(16,16, new UiItemSprite((ItemId)(object)-1))));
        }

        public override Vector2 GetSize() => Vector2.One * 16;
    }
}