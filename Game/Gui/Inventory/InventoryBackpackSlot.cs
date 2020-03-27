using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory
{
    sealed class InventoryBackpackSlot : InventorySlot
    {
        protected override ButtonFrame Frame { get; }
        readonly UiSpriteElement<ItemSpriteId> _sprite;
        readonly int _slotNumber;
        int _version;
        int _frameNumber;
        protected override ItemSlotId SlotId => (ItemSlotId)((int)ItemSlotId.Slot0 + _slotNumber);
        static readonly HandlerSet BackpackHandlers = new HandlerSet(SlotHandlers,
            H<InventoryBackpackSlot, InventoryChangedEvent>((x, e) => x._version++),
            H<InventoryBackpackSlot, SlowClockEvent>((x, e) => x._frameNumber += e.Delta)
        );

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

        public InventoryBackpackSlot(PartyCharacterId activeCharacter, int slotNumber)
            : base(activeCharacter, BackpackHandlers)
        {
            _slotNumber = slotNumber;
            _sprite = new UiSpriteElement<ItemSpriteId>(0) { SubId = (int)ItemSpriteId.Nothing };

            var amountSource = new DynamicText(() =>
            {
                GetSlot(out var slotInfo, out _);
                return slotInfo == null || slotInfo.Amount < 2
                    ? new TextBlock[0]
                    : new[] { new TextBlock(slotInfo.Amount.ToString()) { Alignment = TextAlignment.Right } };
            }, x => _version);

            var text = new TextBlockElement(amountSource);

            Frame = new ButtonFrame(new FixedPositionStack()
                .Add(_sprite, 0, 0, 16, 16)
                .Add(text, 0, 20 - 9, 16, 9)
            )
            {
                Padding = -1,
                Theme = new InventorySlotTheme(),
                State = ButtonState.Pressed
            };

            AttachChild(Frame);
        }

        void Rebuild()
        {
            GetSlot(out _, out var item);

            if(item == null)
            {
                _sprite.SubId = (int)ItemSpriteId.Nothing;
                return;
            }

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

        public override Vector2 GetSize() => new Vector2(16, 20);
    }
}
