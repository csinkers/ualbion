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

        static readonly HandlerSet Handlers = new HandlerSet(
            H<InventoryExitButton, HoverEvent>((x,e) => x._state = ButtonState.Hover),
            H<InventoryExitButton, BlurEvent>((x,e) => x._state = ButtonState.Normal),
            H<InventoryExitButton, UiLeftClickEvent>((x, e) => x._state = ButtonState.Clicked),
            H<InventoryExitButton, UiLeftReleaseEvent>((x, _) =>
            {
                if (x._state == ButtonState.Clicked)
                {
                    x.Raise(new ButtonPressEvent(x._buttonId));
                    x._state = ButtonState.Normal;
                }
            })
        );
        public InventoryExitButton(string buttonId) : base(Handlers) => _buttonId = buttonId;

        public override void Subscribed()
        {
            if (_sprite == null)
                _sprite = AttachChild(new UiSpriteElement<CoreSpriteId>(CoreSpriteId.UiExitButton));
            base.Subscribed();
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
