using System;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.State.Player;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryExitButton : UiElement
    {
        UiSpriteElement<CoreSpriteId> _sprite;
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
