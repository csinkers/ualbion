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
        float _typematicAccrual;
        bool _isPressed;
        bool _isClicked;
        bool _isHovered;

        public Button(IUiElement content)
        {
            On<HoverEvent>(_ => IsHovered = true);
            On<BlurEvent>(_ => IsHovered = false);
            On<UiLeftClickEvent>(e =>
            {
                IsClicked = true;
                if (Typematic)
                    Click?.Invoke();
                e.Propagating = false;
            });

            On<UiLeftReleaseEvent>(e =>
            {
                if (Typematic)
                    _typematicAccrual = 0;
                else if (IsClicked && IsHovered)
                    Click?.Invoke();

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
                    Click?.Invoke();
            });

            _frame = AttachChild(new ButtonFrame(content));
        }

        public Button(IText textSource) : this(new UiText(textSource)) { }
        public Button(StringId textId) : this((IUiElement)new UiTextBuilder(textId).Center().NoWrap()) { }
        public Button(string literalText) : this((IUiElement)new SimpleText(literalText).Center().NoWrap()) { }
        public Button OnClick(Action callback) { Click += callback; return this; }
        // public Button OnRightClick(Action callback) { RightClick += callback; return this; }
        // public Button OnDoubleClick(Action callback) { DoubleClick += callback; return this; }
        // public Button OnPressed(Action callback) { Pressed += callback; return this; }

        event Action Click;
        // event Action RightClick;
        // event Action DoubleClick;
        // event Action Pressed;

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
