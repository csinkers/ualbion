using System.Numerics;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Entities;

namespace UAlbion.Game.Gui.Inventory
{
    class InventoryBodyPart : UiElement
    {
        readonly PartyCharacterId _activeCharacter;
        readonly ItemSlotId _itemSlotId;

        // Inner area 16x16 w/ 1-pixel button frame
        public InventoryBodyPart(PartyCharacterId activeCharacter, ItemSlotId itemSlotId)
        {
            _activeCharacter = activeCharacter;
            _itemSlotId = itemSlotId;
            Children.Add(
                new ButtonFrame(
                    new FixedSize(16,16,
                        new UiItemSprite((ItemId)(object)-1))));
        }

        public override string ToString() => $"InventoryBodyPart:{_itemSlotId}";
        public override Vector2 GetSize() => Vector2.One * 16;
    }
}