using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Gui
{
    class Button : UiElement
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<Button, UiHoverEvent>((x, _) =>
                x._frame.State = x.IsPressed
                    ? ButtonState.HoverPressed
                    : ButtonState.Hover),
            H<Button, UiBlurEvent>((x, _) =>
                x._frame.State = x.IsPressed
                    ? ButtonState.Pressed
                    : ButtonState.Normal),

            H<Button, UiLeftClickEvent>((x, e) =>
            {
                x._frame.State = ButtonState.Clicked;
                e.Propagating = false;
            }),
            H<Button, UiLeftReleaseEvent>((x, _) =>
                {
                    if(x._frame.State == ButtonState.Clicked)
                        x.Raise(new ButtonPressEvent(x.Id));

                    x._frame.State = x.IsPressed
                        ? ButtonState.Pressed
                        : ButtonState.Normal;
                })
            );

        readonly ButtonFrame _frame;
        public string Id { get; }
        public bool IsPressed { get; set; }

        public Button(string buttonId, StringId textId) : base(Handlers)
        {
            Id = buttonId;
            var text = new Text(textId).Center();
            _frame = new ButtonFrame(text);
            Children.Add(_frame);
        }

        public override Vector2 GetSize() => GetMaxChildSize() + new Vector2(4, 0);

        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc)
        {
            // TODO: Emit rectangle/border renderable & hovered state highlight renderable
            var innerExtents = new Rectangle(extents.X + 2, extents.Y, extents.Width - 4, extents.Height);
            return RenderChildren(innerExtents, order, addFunc);
        }

        public override void Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            if (!extents.Contains((int)uiPosition.X, (int)uiPosition.Y))
                return;
            var innerExtents = new Rectangle(extents.X + 2, extents.Y, extents.Width - 4, extents.Height);
            SelectChildren(uiPosition, innerExtents, order, registerHitFunc);
            registerHitFunc(order, this);
        }
    }
}
