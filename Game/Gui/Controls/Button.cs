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
        readonly Action _clickAction;
        float _typematicAccrual;
        bool _isPressed;
        bool _isClicked;
        bool _isHovered;

        public Button(IUiElement content, Action action)
        {
            On<HoverEvent>(_ => IsHovered = true);
            On<BlurEvent>(_ => IsHovered = false);
            On<UiLeftClickEvent>(e =>
            {
                IsClicked = true;
                if (Typematic)
                    _clickAction();
                e.Propagating = false;
            });

            On<UiLeftReleaseEvent>(e =>
            {
                if (Typematic)
                    _typematicAccrual = 0;
                else if (IsClicked && IsHovered)
                    _clickAction();

                IsClicked = false;
            });

            On<EngineUpdateEvent>(e =>
            {
                if (!Typematic || !IsClicked)
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

        public Button(IText textSource, Action action) : this(new UiText(textSource), action) { }
        public Button(StringId textId, Action action) : this((IUiElement)new UiTextBuilder(textId).Center().NoWrap(), action) { }
        public Button(string literalText, Action action) : this((IUiElement)new SimpleText(literalText).Center().NoWrap(), action) { }

        public ButtonFrame.ThemeFunction Theme
        {
            get => _frame.Theme;
            set => _frame.Theme = value;
        }

        public bool DoubleFrame { get; set; }
        public bool Typematic { get; set; }

        public bool IsPressed
        {
            get => _isPressed;
            set
            {
                if (_isPressed == value)
                    return;
                _isPressed = value;
                _frame.State = State;
            }
        }

        bool IsClicked
        {
            get => _isClicked;
            set
            {
                if (_isClicked == value)
                    return;
                _isClicked = value;
                _frame.State = State;
            }
        }

        bool IsHovered
        {
            get => _isHovered;
            set
            {
                if (_isHovered == value)
                    return;
                _isHovered = value;
                _frame.State = State;
            }
        }

        ButtonState State => (IsClicked, IsPressed, IsHovered) switch
        {
            (false, false, false) => ButtonState.Normal,
            (false, false, true) => ButtonState.Hover,
            (false, true, false) => ButtonState.Pressed,
            (false, true, true) => ButtonState.HoverPressed,
            (true, _, false) => ButtonState.ClickedBlurred,
            (true, _, true) => ButtonState.Clicked,
        };

        public override Vector2 GetSize() => GetMaxChildSize() + new Vector2(4, 0);

        protected override int DoLayout(Rectangle extents, int order, Func<IUiElement, Rectangle, int, int> func)
        {
            var innerExtents = new Rectangle(extents.X + 2, extents.Y, extents.Width - 4, extents.Height);
            return base.DoLayout(innerExtents, order, func);
        }
    }
}
