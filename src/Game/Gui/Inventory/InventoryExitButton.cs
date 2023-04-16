using System;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.State.Player;

namespace UAlbion.Game.Gui.Inventory;

public class InventoryExitButton : UiElement
{
    UiSpriteElement _sprite;
    ButtonState _state;
    event Action Click;

    public InventoryExitButton()
    {
        On<HoverEvent>(_ =>
        {
            var im = Resolve<IInventoryManager>();
            if (im.ItemInHand.Item.IsNone)
                _state = ButtonState.Hover;
        });
        On<BlurEvent>(_ => _state = ButtonState.Normal);
        On<UiLeftClickEvent>(_ =>
        {
            var im = Resolve<IInventoryManager>();
            if (im.ItemInHand.Item.IsNone)
                _state = ButtonState.Clicked;
        });
        On<UiLeftReleaseEvent>(_ =>
        {
            if (_state != ButtonState.Clicked)
                return;

            _state = ButtonState.Normal;
            Click?.Invoke();
        });
    }

    public InventoryExitButton OnClick(Action callback) { Click += callback; return this; }

    protected override void Subscribed()
    {
        _sprite ??= AttachChild(new UiSpriteElement(Base.CoreGfx.UiExitButton));
    }

    protected override int DoLayout<T>(Rectangle extents, int order, T context, LayoutFunc<T> func)
    {
        _sprite.Id = _state switch
        {
            ButtonState.Normal  => Base.CoreGfx.UiExitButton,
            ButtonState.Hover   => Base.CoreGfx.UiExitButtonHover,
            ButtonState.Clicked => Base.CoreGfx.UiExitButtonPressed,
            _ => _sprite.Id
        };
        return base.DoLayout(extents, order, context, func);
    }
}