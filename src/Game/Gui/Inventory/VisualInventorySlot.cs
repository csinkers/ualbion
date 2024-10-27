using System;
using System.Numerics;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets.Inv;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory;

public sealed class VisualInventorySlot : UiElement
{
    readonly UiSpriteElement _sprite;
    readonly UiSpriteElement _overlay;
    readonly Func<IReadOnlyItemSlot> _getSlot;
    readonly InventorySlotId _slotId;
    readonly Button _button;
    readonly Vector2 _size;

    int _frameNumber;

    public VisualInventorySlot(InventorySlotId slotId, IText amountSource, Func<IReadOnlyItemSlot> getSlot)
    {
        On<IdleClockEvent>(_ => _frameNumber++);

        _slotId = slotId;
        _getSlot = getSlot;
        _overlay = new UiSpriteElement(Base.CoreGfx.UiBroken) { IsActive = false };
        var text = new UiText(amountSource);

        if (!slotId.Slot.IsSpecial())
        {
            _size = slotId.Slot.IsBodyPart() ?
                new Vector2(18, 18) : //16x16 surrounded by 1px borders
                new Vector2(16, 20);

            _sprite = new UiSpriteElement(SpriteId.None);
            _button = AttachChild(new Button(new FixedPositionStacker()
                    .Add(
                        new LayerStacker(_sprite, _overlay),
                        1, 1, 16, 16) //16x16 surrounded by 1px borders
                    .Add(text, 0, 20 - 9, 16, 9))
                {
                    Padding = -1,
                    Margin = 0,
                    Theme = slotId.Slot.IsBodyPart()
                        ? ButtonTheme.Default
                        : ButtonTheme.InventorySlot,

                    IsPressed = !slotId.Slot.IsBodyPart()
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
            _sprite = new UiSpriteElement(
                slotId.Slot == ItemSlotId.Gold
                    ? Base.CoreGfx.UiGold
                    : Base.CoreGfx.UiFood);

            _button = AttachChild(new Button(
                    new VerticalStacker(
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
    public override Vector2 GetSize() => _sprite.Id.Type == AssetType.CoreGfx ? base.GetSize() : _size;
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
    public bool SuppressNextDoubleClick { get => _button.SuppressNextDoubleClick; set => _button.SuppressNextDoubleClick = value; }

    void Rebuild(in Rectangle extents)
    {
        var slot = _getSlot();
        if (slot == null)
            return;

        _button.AllowDoubleClick = slot.Amount > 1;

        if ((int)slot.LastUiPosition.X != extents.X || (int)slot.LastUiPosition.Y != extents.Y)
            Raise(new SetInventorySlotUiPositionEvent(_slotId, extents.X, extents.Y));

        if (_slotId.Slot.IsSpecial()) // Special slots (i.e. rations + gold) keep their sprite when empty.
        {
            if (_slotId.Slot == ItemSlotId.Gold)
                _sprite.Id = Base.CoreGfx.UiGold;
            else if (_slotId.Slot == ItemSlotId.Rations)
                _sprite.Id = Base.CoreGfx.UiFood;
        }
        else if (slot.Item.Type == AssetType.Item)
        {
            var item = Assets.LoadItem(slot.Item);
            int frames = item.IconAnim == 0 ? 1 : item.IconAnim;
            while (_frameNumber >= frames)
                _frameNumber -= frames;

            int itemSpriteId = item.IconSubId + _frameNumber;
            _sprite.Id = item.Icon;
            _sprite.SubId = itemSpriteId;
            _overlay.IsActive = (slot.Flags & ItemSlotFlags.Broken) != 0;
        }
        else // Special slots (i.e. rations + gold) keep their sprite when empty.
        {
            _sprite.Id = AssetId.None; // Nothing
            _sprite.SubId = 0;
            _overlay.IsActive = false;
        }
    }

    public override int Render(Rectangle extents, int order, LayoutNode parent)
    {
        Rebuild(extents);
        return base.Render(extents, order, parent);
    }
}
