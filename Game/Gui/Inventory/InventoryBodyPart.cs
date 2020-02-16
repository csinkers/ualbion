using System.Numerics;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Entities;
using UAlbion.Game.State;
using Veldrid;

namespace UAlbion.Game.Gui.Inventory
{
    sealed class InventoryBodyPart : InventorySlot
    {
        static readonly HandlerSet BackpackHandlers = new HandlerSet(SlotHandlers,
            H<InventoryBodyPart, SlowClockEvent>((x, e) => x._frameNumber += e.Delta)
        );

        protected override ButtonFrame Frame { get; }
        protected override ItemSlotId SlotId { get; }
        readonly UiSpriteElement<ItemSpriteId> _sprite;
        int _frameNumber;

        // Inner area 16x16 w/ 1-pixel button frame
        public InventoryBodyPart(PartyCharacterId activeCharacter, ItemSlotId itemSlotId)
            : base(activeCharacter, SlotHandlers)
        {
            SlotId = itemSlotId;
            _sprite = new UiSpriteElement<ItemSpriteId>(0) { SubId = (int)ItemSpriteId.Nothing };
            Frame = new ButtonFrame(new FixedSize(16, 16, _sprite)) { Padding = -1 };
            AttachChild(Frame);
        }

        void Rebuild()
        {
            var state = Resolve<IGameState>();
            var assets = Resolve<IAssetManager>();
            var member = state.Party[ActiveCharacter];
            var slotInfo = member?.Apparent.Inventory.GetSlot(SlotId);

            if(slotInfo == null)
            {
                _sprite.SubId = (int)ItemSpriteId.Nothing;
                return;
            }

            var item = assets.LoadItem(slotInfo.Id);

            int frames = item.IconAnim == 0 ? 1 : item.IconAnim;
            while (_frameNumber >= frames)
                _frameNumber -= frames;

            int itemSpriteId = (int)item.Icon + _frameNumber;
            _sprite.SubId = itemSpriteId;
            // TODO: Show item.Amount
            // TODO: Show broken overlay if item.Flags.HasFlag(ItemSlotFlags.Broken)
        }

        public override int Render(Rectangle extents, int order)
        {
            Rebuild();
            return base.Render(extents, order);
        }

        public override string ToString() => $"InventoryBodyPart:{SlotId}";
        public override Vector2 GetSize() => Vector2.One * 16;
    }
}
