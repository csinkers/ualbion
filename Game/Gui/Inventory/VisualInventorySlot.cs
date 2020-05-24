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
        readonly UiSpriteElement<ItemSpriteId> _sprite;
        readonly Func<ItemData> _getItem;
        readonly Vector2 _size;

        int _frameNumber;
        bool _isClickTimerPending;

        public VisualInventorySlot(bool isBodyPart, IText amountSource, Func<ItemData> getItem)
        {
            On<UiLeftClickEvent>(OnClick);
            On<UiRightClickEvent>(OnRightClicked);
            On<IdleClockEvent>(e => _frameNumber++);
            On<TimerElapsedEvent>(e =>
            {
                if (e.Id == TimerName)
                    OnTimer();
            });

            _getItem = getItem;
            _size = isBodyPart ? new Vector2(16, 16) : new Vector2(16, 20);
            _sprite = new UiSpriteElement<ItemSpriteId>(0) { SubId = (int)ItemSpriteId.Nothing };
            var text = new UiText(amountSource);

            _frame = AttachChild(new ButtonFrame(new FixedPositionStack()
                .Add(_sprite, 0, 0, 16, 16)
                .Add(text, 0, 20 - 9, 16, 9))
            {
                Padding = -1,
                Theme = isBodyPart ? (ButtonFrame.ThemeFunction)ButtonTheme.Default : ButtonTheme.InventorySlot,
                State = isBodyPart ? ButtonState.Normal : ButtonState.Pressed
            });
        }

        public ButtonState State { get => _frame.State; set => _frame.State = value; }
        public override Vector2 GetSize() => _size;
        public event EventHandler<EventArgs> Clicked;
        public event EventHandler<EventArgs> DoubleClicked;
        public event EventHandler<EventArgs> RightClicked;
        void Rebuild()
        {
            var item = _getItem();
            if (item == null)
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