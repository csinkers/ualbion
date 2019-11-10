using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Gui
{
    public class ImageButton : UiElement
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<ImageButton, UiHoverEvent>((x, _) =>
            {
                if (x._frame.State == ButtonState.ClickedBlurred)
                    x._frame.State = ButtonState.Clicked;
                else
                    x._frame.State = x.IsPressed
                        ? ButtonState.HoverPressed
                        : ButtonState.Hover;
            }),
            H<ImageButton, UiBlurEvent>((x, _) =>
            {
                if (x._frame.State == ButtonState.Clicked)
                    x._frame.State = ButtonState.ClickedBlurred;
                else
                    x._frame.State = x.IsPressed
                        ? ButtonState.Pressed
                        : ButtonState.Normal;
            }),

            H<ImageButton, UiLeftClickEvent>((x, e) =>
            {
                x._frame.State = ButtonState.Clicked;
                if (x.Typematic)
                    x.Raise(new ButtonPressEvent(x.Id));
                e.Propagating = false;
                x.Raise(new SetExclusiveMouseModeEvent(x));
            }),

            H<ImageButton, UiLeftReleaseEvent>((x, _) =>
            {
                if (x.Typematic)
                    x._typematicAccrual = 0;
                else if (x._frame.State == ButtonState.Clicked)
                    x.Raise(new ButtonPressEvent(x.Id));

                x._frame.State = x.IsPressed
                    ? ButtonState.Pressed
                    : ButtonState.Normal;
            }),

            H<ImageButton, EngineUpdateEvent>((x, e) =>
            {
                if (!x.Typematic || x._frame.State != ButtonState.Clicked)
                    return;

                var oldAccrual = x._typematicAccrual;
                x._typematicAccrual += e.DeltaSeconds;
                var rate = 8 * (int)(2 * oldAccrual);
                var oldAmount = (int)(oldAccrual * rate);
                var newAmount = (int)(x._typematicAccrual * rate);
                var delta = newAmount - oldAmount;
                var clickEvent = new ButtonPressEvent(x.Id);
                for (int i = 0; i < delta; i++)
                    x.Raise(clickEvent);
            }));

        readonly ButtonFrame _frame;
        public string Id { get; }
        public bool IsPressed { get; set; }
        public bool DoubleFrame { get; set; }
        public bool Typematic { get; set; }
        float _typematicAccrual;

        public ImageButton(string buttonId, StringId textId) : base(Handlers)
        {
            Id = buttonId;
            var text = new Text(textId).Center();
            _frame = new ButtonFrame(text);
            Children.Add(_frame);
        }

        public ImageButton(string buttonId, string literalText) : base(Handlers)
        {
            Id = buttonId;
            var text = new Text(literalText).Center();
            _frame = new ButtonFrame(text);
            Children.Add(_frame);
        }

        public override Vector2 GetSize() => GetMaxChildSize() + new Vector2(4, 0);

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            // TODO: Emit rectangle/border renderable & hovered state highlight renderable
            var innerExtents = new Rectangle(extents.X + 2, extents.Y, extents.Width - 4, extents.Height);
            return base.DoLayout(innerExtents, order, func);
        }
    }
}