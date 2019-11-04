using System;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryExitButton : UiElement
    {
        readonly string _buttonId;
        UiSprite<CoreSpriteId> _normal;
        UiSprite<CoreSpriteId> _hover;
        UiSprite<CoreSpriteId> _clicked;
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

        protected override void Subscribed()
        {
            _normal = new UiSprite<CoreSpriteId>(CoreSpriteId.UiExitButton);
            _hover = new UiSprite<CoreSpriteId>(CoreSpriteId.UiExitButtonHover);
            _clicked = new UiSprite<CoreSpriteId>(CoreSpriteId.UiExitButtonPressed);
            Exchange
                .Attach(_normal)
                .Attach(_hover)
                .Attach(_clicked);
            Children.Add(_normal);
            Children.Add(_hover);
            Children.Add(_clicked);
            base.Subscribed();
        }

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc) =>
            _state switch
            {
                ButtonState.Normal => _normal.Render(extents, order, addFunc),
                ButtonState.Hover => _hover.Render(extents, order, addFunc),
                ButtonState.Clicked => _clicked.Render(extents, order, addFunc),
                _ => order
            };
    }
}