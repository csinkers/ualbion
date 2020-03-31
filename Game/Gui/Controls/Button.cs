using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Controls
{
    class Button : UiElement
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<Button, HoverEvent>((x, _) =>
            {
                if (x._frame.State == ButtonState.ClickedBlurred)
                    x._frame.State = ButtonState.Clicked;
                else
                    x._frame.State = x.IsPressed
                        ? ButtonState.HoverPressed
                        : ButtonState.Hover;
            }),
            H<Button, BlurEvent>((x, _) =>
            {
                if (x._frame.State == ButtonState.Clicked)
                    x._frame.State = ButtonState.ClickedBlurred;
                else
                    x._frame.State = x.IsPressed
                        ? ButtonState.Pressed
                        : ButtonState.Normal;
            }),

            H<Button, UiLeftClickEvent>((x, e) =>
            {
                x._frame.State = ButtonState.Clicked;
                if (x.Typematic)
                    x._clickAction();
                e.Propagating = false;
                x.Raise(new SetExclusiveMouseModeEvent(x));
            }),

            H<Button, UiLeftReleaseEvent>((x, _) =>
            {
                if (x.Typematic)
                    x._typematicAccrual = 0;
                else if (x._frame.State == ButtonState.Clicked)
                    x._clickAction();

                x._frame.State = x.IsPressed
                    ? ButtonState.Pressed
                    : ButtonState.Normal;
            }),

            H<Button, EngineUpdateEvent>((x, e) =>
            {
                if (!x.Typematic || x._frame.State != ButtonState.Clicked)
                    return;

                var oldAccrual = x._typematicAccrual;
                x._typematicAccrual += e.DeltaSeconds;
                var rate = 8 * (int)(2 * oldAccrual);
                var oldAmount = (int)(oldAccrual * rate);
                var newAmount = (int)(x._typematicAccrual * rate);
                var delta = newAmount - oldAmount;
                for (int i = 0; i < delta; i++)
                    x._clickAction();
            }));

        readonly ButtonFrame _frame;
        Action _clickAction { get; }
        public ButtonFrame.ITheme Theme
        {
            get => _frame.Theme;
            set => _frame.Theme = value;
        }

        public bool IsPressed
        {
            get => _isPressed;
            set
            {
                if (_isPressed == value)
                    return;
                _isPressed = value;
                _frame.State = IsPressed ? ButtonState.Pressed : ButtonState.Normal;
            }
        }

        public bool DoubleFrame { get; set; }
        public bool Typematic { get; set; }
        float _typematicAccrual;
        bool _isPressed;

        public Button(IUiElement content, Action action) : base(Handlers)
        {
            _frame = AttachChild(new ButtonFrame(content));
            _clickAction = action;
        }
        public Button(IText textSource, Action action) : this(new TextElement(textSource), action) { }
        public Button(StringId textId, Action action) : this(new TextElement(textId).Center().NoWrap(), action) { }
        public Button(string literalText, Action action) : this(new TextElement(literalText).Center().NoWrap(), action) { }

        public override Vector2 GetSize() => GetMaxChildSize() + new Vector2(4, 0);

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            var innerExtents = new Rectangle(extents.X + 2, extents.Y, extents.Width - 4, extents.Height);
            return base.DoLayout(innerExtents, order, func);
        }
    }
}
