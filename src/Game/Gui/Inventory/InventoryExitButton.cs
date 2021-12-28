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
        On<HoverEvent>(e =>
        {
            var im = Resolve<IInventoryManager>();
            if (im?.ItemInHand?.Item == null)
                _state = ButtonState.Hover;
        });
        On<BlurEvent>(e => _state = ButtonState.Normal);
        On<UiLeftClickEvent>(e =>
        {
            var im = Resolve<IInventoryManager>();
            if (im?.ItemInHand?.Item == null)
                _state = ButtonState.Clicked;
        });
        On<UiLeftReleaseEvent>(e =>
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
        _sprite ??= AttachChild(new UiSpriteElement(Base.CoreSprite.UiExitButton));
    }

    protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
    {
        _sprite.Id = _state switch
        {
            ButtonState.Normal  => Base.CoreSprite.UiExitButton,
            ButtonState.Hover   => Base.CoreSprite.UiExitButtonHover,
            ButtonState.Clicked => Base.CoreSprite.UiExitButtonPressed,
            _ => _sprite.Id
        };
        return base.DoLayout(extents, order, func);
    }
}