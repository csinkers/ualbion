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
    class InventorySlot : UiElement
    {
        readonly PartyCharacterId _activeCharacter;
        readonly int _slotNumber;
        readonly ButtonFrame _frame;
        readonly UiItemSprite _sprite;
        int _version;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<InventorySlot, InventoryChangedEvent>((x, e) => x._version++),
            H<InventorySlot, UiHoverEvent>((x, e) =>
            {
                x.Hover(); 
                e.Propagating = false;
            }),
            H<InventorySlot, UiBlurEvent>((x, _) =>
            {
                x._frame.State = ButtonState.Pressed;
                x.Raise(new HoverTextEvent(""));
            })
        );


        void Hover()
        {
            var state = Resolve<IStateManager>();
            var assets = Resolve<IAssetManager>();
            var settings = Resolve<ISettings>();

            var member = state.State.GetPartyMember(_activeCharacter);
            var slotInfo = member.Inventory.GetSlot((ItemSlotId)((int)ItemSlotId.Slot0 + _slotNumber));
            if (slotInfo == null)
                return;

            var item = assets.LoadItem(slotInfo.Id);
            if (item == null)
                return;

            _frame.State = ButtonState.HoverPressed;
            var text = item.GetName(settings.Language);
            Raise(new HoverTextEvent(text));
        }

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

        public InventorySlot(PartyCharacterId activeCharacter, int slotNumber)
            : base(Handlers)
        {
            _activeCharacter = activeCharacter;
            _slotNumber = slotNumber;
            _sprite = new UiItemSprite(ItemSpriteId.Nothing);

            var amountSource = new DynamicText(() =>
            {
                GetSlot(out _, out var slotInfo, out _);
                return slotInfo == null || slotInfo.Amount < 2 
                    ? new TextBlock[0] 
                    : new[] { new TextBlock(slotInfo.Amount.ToString()) { Alignment = TextAlignment.Right } };
            }, x => _version);

            var text = new Text(amountSource);

            _frame = new ButtonFrame(new FixedPositionStack()
                .Add(_sprite, 0, 0, 16, 16)
                .Add(text, 0, 20 - 9, 16, 9)
            )
            {
                Padding = -1,
                Theme = new InventorySlotTheme(),
                State = ButtonState.Pressed
            };

            Children.Add(_frame);
        }

        void GetSlot(out ICharacterSheet member, out ItemSlot slotInfo, out ItemData item)
        {
            var state = Resolve<IStateManager>();
            var assets = Resolve<IAssetManager>();
            member = state.State.GetPartyMember(_activeCharacter);
            slotInfo = member.Inventory.GetSlot((ItemSlotId)((int)ItemSlotId.Slot0 + _slotNumber));
            item = slotInfo == null ? null : assets.LoadItem(slotInfo.Id);
        }

        void Rebuild()
        {
            var state = Resolve<IStateManager>();
            GetSlot(out _, out _, out var item);

            if(item == null)
            {
                _sprite.Id = ItemSpriteId.Nothing;
                return;
            }

            int frames = item.IconAnim == 0 ? 1 : item.IconAnim;
            int sprite = (int)item.Icon + state.FrameCount % frames;
            _sprite.Id = (ItemSpriteId)sprite;
            // TODO: Show item.Amount
            // TODO: Show broken overlay if item.Flags.HasFlag(ItemSlotFlags.Broken)
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            Rebuild();
            return base.Render(extents, order, addFunc);
        }

        public override Vector2 GetSize() => new Vector2(16, 20);
    }
}