using System;
using System.Threading;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Controls
{
    public class Button : UiElement
    {
        const int DoubleClickIntervalMilliseconds = 250;
        string TimerName => "DoubleClickButton." + _id;

        static int _nextId;
        readonly ButtonFrame _frame;
        readonly int _id;
        float _typematicAccrual;
        bool _isPressed;
        bool _isClicked;
        bool _isHovered;
        bool _isClickTimerPending;
        bool _hoverable = true;

        public Button(IUiElement content)
        {
            On<HoverEvent>(_ => { IsHovered = true; Hover?.Invoke(); });
            On<BlurEvent>(_ => { IsHovered = false; Blur?.Invoke(); });
            On<UiLeftClickEvent>(e =>
            {
                if(!IsClicked)
                    ButtonDown?.Invoke();

                IsClicked = true;
                if (Typematic)
                    Click?.Invoke();

                e.Propagating = false;
            });

            On<UiLeftReleaseEvent>(e =>
            {
                if (!IsClicked)
                    return;
                IsClicked = false;

                if (Typematic)
                {
                    _typematicAccrual = 0;
                    return;
                }

                if (!IsHovered)
                    return;

                if (DoubleClick == null) // Simple single click only button
                {
                    Click?.Invoke();
                    return;
                }

                if (_isClickTimerPending) // If they double-clicked...
                {
                    DoubleClick?.Invoke();
                    _isClickTimerPending = false; // Ensure the single-click behaviour doesn't happen.
                }
                else // For the first click, just start the double-click timer.
                {
                    Raise(new StartTimerEvent(TimerName, DoubleClickIntervalMilliseconds, this));
                    _isClickTimerPending = true;
                }
            });

            On<UiRightClickEvent>(e =>
            {
                e.Propagating = false;
                RightClick?.Invoke();
            });

            On<TimerElapsedEvent>(e =>
            {
                if (e.Id != TimerName)
                    return;

                if (!_isClickTimerPending) // They've already double-clicked
                    return;

                Click?.Invoke();
                _isClickTimerPending = false;
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

            _id = Interlocked.Increment(ref _nextId);
            _frame = AttachChild(new ButtonFrame(content));
        }

        public Button(IText textSource) : this(new UiText(textSource)) { }
        public Button(StringId textId) : this((IUiElement)new UiTextBuilder(textId).Center().NoWrap()) { }
        public Button(string literalText) : this((IUiElement)new SimpleText(literalText).Center().NoWrap()) { }
        public Button OnClick(Action callback) { Click += callback; return this; }
        public Button OnRightClick(Action callback) { RightClick += callback; return this; }
        public Button OnDoubleClick(Action callback) { DoubleClick += callback; return this; }
        public Button OnButtonDown(Action callback) { ButtonDown += callback; return this; }
        public Button OnHover(Action callback) { Hover += callback; return this; } 
        public Button OnBlur(Action callback) { Blur += callback; return this; } 
        event Action Click;
        event Action RightClick;
        event Action DoubleClick;
        event Action ButtonDown;
        event Action Hover;
        event Action Blur;

        public ButtonFrame.ThemeFunction Theme { get => _frame.Theme; set => _frame.Theme = value; }
        public int Padding { get => _frame.Padding; set => _frame.Padding = value; } 
        public bool DoubleFrame { get; set; }
        public bool Typematic { get; set; }
        public bool Hoverable { get => _hoverable; set { _hoverable = value; _frame.State = State; } }

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
            (false, false, true) => Hoverable ? ButtonState.Hover : ButtonState.Normal,
            (false, true, false) => ButtonState.Pressed,
            (false, true, true) => Hoverable ? ButtonState.HoverPressed : ButtonState.Pressed,
            (true, _, false) => ButtonState.ClickedBlurred,
            (true, _, true) => ButtonState.Clicked,
        };
    }
}
