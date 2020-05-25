using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory
{
    public sealed class VisualInventorySlot : UiElement
    {
        const string TimerName = "InventorySlot.ClickTimer";

        readonly ButtonFrame _frame;
        readonly UiSpriteElement<AssetId> _sprite;
        readonly UiSpriteElement<AssetId> _overlay;
        readonly Func<ItemSlot> _getSlot;
        readonly Vector2 _size;

        int _frameNumber;
        bool _isClickTimerPending;

        ItemData Item
        {
            get
            {
                var slot = _getSlot();
                return slot?.Id == null ? null : Resolve<IAssetManager>().LoadItem(slot.Id.Value);
            }
        }

        public VisualInventorySlot(ItemSlotId slotId, IText amountSource, Func<ItemSlot> getSlot)
        {
            On<UiLeftClickEvent>(OnClick);
            On<UiRightClickEvent>(OnRightClicked);
            On<IdleClockEvent>(e => _frameNumber++);
            On<TimerElapsedEvent>(e =>
            {
                if (e.Id == TimerName)
                    OnTimer();
            });

            _getSlot = getSlot;
            _overlay = new UiSpriteElement<AssetId>(CoreSpriteId.UiBroken.ToAssetId()) { IsActive = false };
            var text = new UiText(amountSource);

            if (!slotId.IsSpecial())
            {
                _size = slotId.IsBodyPart() ? new Vector2(16, 16) : new Vector2(16, 20);
                _sprite = new UiSpriteElement<AssetId>(ItemSpriteId.Nothing.ToAssetId())
                {
                    SubId = (int)ItemSpriteId.Nothing
                };

                _frame = AttachChild(new ButtonFrame(new FixedPositionStack()
                    .Add(
                        new LayerStack(
                            _sprite,
                            _overlay),
                        0, 0, 16, 16)
                    .Add(text, 0, 20 - 9, 16, 9))
                {
                    Padding = -1,
                    Theme = slotId.IsBodyPart()
                        ? (ButtonFrame.ThemeFunction)ButtonTheme.Default
                        : ButtonTheme.InventorySlot,
                    State = slotId.IsBodyPart() ? ButtonState.Normal : ButtonState.Pressed
                });
            }
            else
            {
                _sprite = new UiSpriteElement<AssetId>(
                    slotId == ItemSlotId.Gold
                        ? CoreSpriteId.UiGold.ToAssetId()
                        : CoreSpriteId.UiFood.ToAssetId());

                _frame = AttachChild(new ButtonFrame(
                    new VerticalStack(
                        new Spacing(31, 0),
                        _sprite,
                        new UiText(amountSource)
                    ) { Greedy = false }
                ));
            }
/*

            _frame = AttachChild(new ButtonFrame(
                new VerticalStack(
                    new Spacing(64, 0),
                    new UiSpriteElement<CoreSpriteId>(CoreSpriteId.UiGold) { Flags = SpriteFlags.Highlight },
                    new UiText(amountSource) 
                ) { Greedy = false}, () => { }
            ) { IsPressed = true });
*/
        }

        public ButtonState State { get => _frame.State; set => _frame.State = value; }
        public override Vector2 GetSize() => _sprite.Id.Type == AssetType.CoreGraphics ? base.GetSize() : _size;
        public event EventHandler<EventArgs> Clicked;
        public event EventHandler<EventArgs> DoubleClicked;
        public event EventHandler<EventArgs> RightClicked;

        void Rebuild()
        {
            var slot = _getSlot();
            var item = Item;
            if (item == null)
            {
                _sprite.SubId = (int)ItemSpriteId.Nothing;
                _overlay.IsActive = false;
                return;
            }

            int frames = item.IconAnim == 0 ? 1 : item.IconAnim;
            while (_frameNumber >= frames)
                _frameNumber -= frames;

            int itemSpriteId = (int)item.Icon + _frameNumber;
            _sprite.SubId = itemSpriteId;
            _overlay.IsActive = (slot.Flags & ItemSlotFlags.Broken) != 0;
        }

        public override int Render(Rectangle extents, int order)
        {
            Rebuild();
            return base.Render(extents, order);
        }

        void OnClick(UiLeftClickEvent e)
        {
            e.Propagating = false;
            if (_isClickTimerPending) // If they double-clicked...
            {
                DoubleClicked?.Invoke(this, EventArgs.Empty);
                _isClickTimerPending = false; // Ensure the single-click behaviour doesn't happen.
            }
            else // For the first click, just start the double-click timer.
            {
                // TODO: If single item, fire the pickup event immediately
                Raise(new StartTimerEvent(TimerName, 300, this));
                _isClickTimerPending = true;
            }
        }

        void OnTimer()
        {
            if (!_isClickTimerPending) // They've already double-clicked
                return;

            Clicked?.Invoke(this, EventArgs.Empty);
            _isClickTimerPending = false;
        }

        void OnRightClicked(UiRightClickEvent e)
        {
            e.Propagating = false;
            RightClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}