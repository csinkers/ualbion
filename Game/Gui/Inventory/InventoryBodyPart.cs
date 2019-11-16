using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.State;
using Veldrid;

namespace UAlbion.Game.Gui.Inventory
{
    class InventoryBodyPart : UiElement
    {
        readonly PartyCharacterId _activeCharacter;
        readonly ItemSlotId _itemSlotId;
        readonly ButtonFrame _frame;
        readonly UiItemSprite _sprite;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<InventoryBodyPart, UiHoverEvent>((x, e) =>
            {
                x.Hover(); 
                e.Propagating = false;
            }),
            H<InventoryBodyPart, UiBlurEvent>((x, _) =>
            {
                x._frame.State = ButtonState.Normal;
                x.Raise(new HoverTextEvent(""));
            })
        );

        // Inner area 16x16 w/ 1-pixel button frame
        public InventoryBodyPart(PartyCharacterId activeCharacter, ItemSlotId itemSlotId)
            : base(Handlers)
        {
            _activeCharacter = activeCharacter;
            _itemSlotId = itemSlotId;
            _sprite = new UiItemSprite(ItemSpriteId.Nothing);
            _frame = new ButtonFrame(new FixedSize(16, 16, _sprite)) { Padding = -1 };
            Children.Add(_frame);
        }

        void Hover()
        {
            var state = Resolve<IStateManager>();
            var assets = Resolve<IAssetManager>();
            var settings = Resolve<ISettings>();

            var member = state.State.GetPartyMember(_activeCharacter);
            var slotInfo = member.Inventory.GetSlot(_itemSlotId);
            if (slotInfo == null)
                return;

            var item = assets.LoadItem(slotInfo.Id);
            if (item == null)
                return;

            _frame.State = ButtonState.Hover;
            var text = item.GetName(settings.Language);
            Raise(new HoverTextEvent(text));
        }

        void Rebuild()
        {
            var state = Resolve<IStateManager>();
            var assets = Resolve<IAssetManager>();
            var member = state.State.GetPartyMember(_activeCharacter);
            var slotInfo = member.Inventory.GetSlot(_itemSlotId);

            if(slotInfo == null)
            {
                _sprite.Id = ItemSpriteId.Nothing;
                return;
            }

            var item = assets.LoadItem(slotInfo.Id);
            int sprite = (int)item.Icon + state.FrameCount % item.IconAnim;
            _sprite.Id = (ItemSpriteId)sprite;
            // TODO: Show item.Amount
            // TODO: Show broken overlay if item.Flags.HasFlag(ItemSlotFlags.Broken)
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            Rebuild();
            return base.Render(extents, order, addFunc);
        }

        public override string ToString() => $"InventoryBodyPart:{_itemSlotId}";
        public override Vector2 GetSize() => Vector2.One * 16;
    }
}