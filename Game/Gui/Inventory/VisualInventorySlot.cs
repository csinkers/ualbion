﻿using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory
{
    public sealed class VisualInventorySlot : UiElement
    {
        // const string TimerName = "InventorySlot.ClickTimer";
        // readonly ButtonFrame _frame;
        readonly UiSpriteElement<AssetId> _sprite;
        readonly UiSpriteElement<AssetId> _overlay;
        readonly Func<IReadOnlyItemSlot> _getSlot;
        readonly Vector2 _size;

        int _frameNumber;

        readonly Button _button;
        // bool _isClickTimerPending;

        ItemData Item => _getSlot().Item as ItemData;

        public VisualInventorySlot(ItemSlotId slotId, IText amountSource, Func<IReadOnlyItemSlot> getSlot)
        {
            // On<UiLeftClickEvent>(OnClick);
            // On<UiRightClickEvent>(OnRightClicked);
            On<IdleClockEvent>(e => _frameNumber++);
            /* On<TimerElapsedEvent>(e =>
            {
                if (e.Id == TimerName)
                    OnTimer();
            }); */

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

                _button = AttachChild(new Button(new FixedPositionStack()
                        .Add(
                            new LayerStack(
                                _sprite,
                                _overlay),
                            0, 0, 16, 16)
                        .Add(text, 0, 20 - 9, 16, 9))
                    {
                        Padding = -1,
                        Margin = 0,
                        Theme = slotId.IsBodyPart()
                            ? (ButtonFrame.ThemeFunction) ButtonTheme.Default
                            : ButtonTheme.InventorySlot,

                        IsPressed = !slotId.IsBodyPart()
                    }
                    .OnHover(() => Hover?.Invoke())
                    .OnBlur(() => Blur?.Invoke())
                    .OnClick(() => Click?.Invoke())
                    .OnRightClick(() => RightClick?.Invoke())
                    .OnDoubleClick(() => DoubleClick?.Invoke())
                    .OnButtonDown(() => ButtonDown?.Invoke()));
            }
            else
            {
                _sprite = new UiSpriteElement<AssetId>(
                    slotId == ItemSlotId.Gold
                        ? CoreSpriteId.UiGold.ToAssetId()
                        : CoreSpriteId.UiFood.ToAssetId());

                _button = AttachChild(new Button(
                    new VerticalStack(
                        new Spacing(31, 0),
                        _sprite,
                        new UiText(amountSource)
                    ) { Greedy = false })
                    .OnHover(() => Hover?.Invoke())
                    .OnBlur(() => Blur?.Invoke())
                    .OnClick(() => Click?.Invoke())
                    .OnRightClick(() => RightClick?.Invoke())
                    .OnDoubleClick(() => DoubleClick?.Invoke())
                    .OnButtonDown(() => ButtonDown?.Invoke()));
            }
        }

        // public ButtonState State { get => _frame.State; set => _frame.State = value; }
        public override Vector2 GetSize() => _sprite.Id.Type == AssetType.CoreGraphics ? base.GetSize() : _size;
        public VisualInventorySlot OnClick(Action callback) { Click += callback; return this; }
        public VisualInventorySlot OnRightClick(Action callback) { RightClick += callback; return this; }
        public VisualInventorySlot OnDoubleClick(Action callback) { DoubleClick += callback; return this; }
        public VisualInventorySlot OnButtonDown(Action callback) { ButtonDown += callback; return this; }
        public VisualInventorySlot OnHover(Action callback) { Hover += callback; return this; } 
        public VisualInventorySlot OnBlur(Action callback) { Blur += callback; return this; } 
        event Action Click;
        event Action DoubleClick;
        event Action RightClick;
        event Action ButtonDown;
        event Action Hover;
        event Action Blur;

        public bool Hoverable { get => _button.Hoverable; set => _button.Hoverable = value; }

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
            _button.AllowDoubleClick = slot.Amount > 1;
        }

        public override int Render(Rectangle extents, int order)
        {
            Rebuild();
            return base.Render(extents, order);
        }
/*
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
        }*/
    }
}