using System;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryExitButton : UiElement
    {
        readonly string _buttonId;
        UiSpriteElement<CoreSpriteId> _sprite;
        ButtonState _state;

        public InventoryExitButton(string buttonId)
        {
            On<HoverEvent>(e => _state = ButtonState.Hover);
            On<BlurEvent>(e => _state = ButtonState.Normal);
            On<UiLeftClickEvent>(e => _state = ButtonState.Clicked);
            On<UiLeftReleaseEvent>(e =>
            {
                if (_state == ButtonState.Clicked)
                {
                    Raise(new ButtonPressEvent(_buttonId));
                    _state = ButtonState.Normal;
                }
            });

            _buttonId = buttonId;
        }

        protected override void Subscribed()
        {
            if (_sprite == null)
                _sprite = AttachChild(new UiSpriteElement<CoreSpriteId>(CoreSpriteId.UiExitButton));
        }

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            _sprite.Id = _state switch
                {
                    ButtonState.Normal  => CoreSpriteId.UiExitButton,
                    ButtonState.Hover   => CoreSpriteId.UiExitButtonHover,
                    ButtonState.Clicked => CoreSpriteId.UiExitButtonPressed,
                    _ => _sprite.Id
                };
            return base.DoLayout(extents, order, func);
        }
    }
}
