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
        readonly ButtonFrame _frame;
        Action _clickAction { get; }
        float _typematicAccrual;
        bool _isPressed;

        public Button(IUiElement content, Action action)
        {
            On<HoverEvent>(e =>
            {
                if (_frame.State == ButtonState.ClickedBlurred)
                    _frame.State = ButtonState.Clicked;
                else
                    _frame.State = IsPressed
                        ? ButtonState.HoverPressed
                        : ButtonState.Hover;
            });
            On<BlurEvent>(e =>
            {
                if (_frame.State == ButtonState.Clicked)
                    _frame.State = ButtonState.ClickedBlurred;
                else
                    _frame.State = IsPressed
                        ? ButtonState.Pressed
                        : ButtonState.Normal;
            });

            On<UiLeftClickEvent>(e =>
            {
                _frame.State = ButtonState.Clicked;
                if (Typematic)
                    _clickAction();
                e.Propagating = false;
                // Raise(new SetExclusiveMouseModeEvent(this));
            });

            On<UiLeftReleaseEvent>(e =>
            {
                if (Typematic)
                    _typematicAccrual = 0;
                else if (_frame.State == ButtonState.Clicked)
                    _clickAction();

                _frame.State = IsPressed
                    ? ButtonState.Pressed
                    : ButtonState.Normal;
            });

            On<EngineUpdateEvent>(e =>
            {
                if (!Typematic || _frame.State != ButtonState.Clicked)
                    return;

                var oldAccrual = _typematicAccrual;
                _typematicAccrual += e.DeltaSeconds;
                var rate = 8 * (int)(2 * oldAccrual);
                var oldAmount = (int)(oldAccrual * rate);
                var newAmount = (int)(_typematicAccrual * rate);
                var delta = newAmount - oldAmount;
                for (int i = 0; i < delta; i++)
                    _clickAction();
            });

            _frame = AttachChild(new ButtonFrame(content));
            _clickAction = action;
        }
        public Button(IText textSource, Action action) : this(new TextElement(textSource), action) { }
        public Button(StringId textId, Action action) : this(new TextElement(textId).Center().NoWrap(), action) { }
        public Button(string literalText, Action action) : this(new TextElement(literalText).Center().NoWrap(), action) { }

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
        public override Vector2 GetSize() => GetMaxChildSize() + new Vector2(4, 0);

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            var innerExtents = new Rectangle(extents.X + 2, extents.Y, extents.Width - 4, extents.Height);
            return base.DoLayout(innerExtents, order, func);
        }
    }
}
