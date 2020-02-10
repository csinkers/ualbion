using System;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryExitButton : UiElement
    {
        readonly string _buttonId;
        UiSpriteElement<CoreSpriteId> _normal;
        UiSpriteElement<CoreSpriteId> _hover;
        UiSpriteElement<CoreSpriteId> _clicked;
        ButtonState _state;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<InventoryExitButton, UiHoverEvent>((x,e) => x._state = ButtonState.Hover),
            H<InventoryExitButton, UiBlurEvent>((x,e) => x._state = ButtonState.Normal),
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
        public InventoryExitButton(string buttonId) : base(Handlers)
        {
            _buttonId = buttonId;
        }

        public override void Subscribed()
        {
            _normal = new UiSpriteElement<CoreSpriteId>(CoreSpriteId.UiExitButton);
            _hover = new UiSpriteElement<CoreSpriteId>(CoreSpriteId.UiExitButtonHover);
            _clicked = new UiSpriteElement<CoreSpriteId>(CoreSpriteId.UiExitButtonPressed);
            Exchange
                .Attach(_normal)
                .Attach(_hover)
                .Attach(_clicked);
            Children.Add(_normal);
            Children.Add(_hover);
            Children.Add(_clicked);
            base.Subscribed();
        }

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func) =>
            _state switch
            {
                ButtonState.Normal => func(_normal, extents, order),
                ButtonState.Hover => func(_hover, extents, order),
                ButtonState.Clicked => func(_clicked, extents, order),
                _ => order
            };
    }
}
