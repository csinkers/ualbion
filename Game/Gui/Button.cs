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
                x._frame.State = x._frame.State == ButtonState.Pressed
                    ? ButtonState.HoverPressed
                    : ButtonState.Hover),
            H<Button, UiBlurEvent>((x, _) =>
                x._frame.State = x._frame.State == ButtonState.HoverPressed
                    ? ButtonState.Pressed
                    : ButtonState.Normal)
        );

        readonly ButtonFrame _frame;

        public Button(StringId textId) : base(Handlers)
        {
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

    public class UiLeftClickEvent : UiEvent { } // Target event, never broadcast
    public class UiLeftReleaseEvent : UiEvent { } // Target event, never broadcast
    public class UiRightClickEvent : UiEvent { } // Target event, never broadcast
    public class UiRightReleaseEvent : UiEvent { } // Target event, never broadcast
    public class UiHoverEvent : UiEvent { } // Targeted event, never broadcast
    public class UiBlurEvent : UiEvent { } // Targeted event, never broadcast
    public abstract class UiEvent : GameEvent { }
    public interface IUiEvent : IGameEvent { }
}
