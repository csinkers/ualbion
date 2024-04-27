﻿using System;
using System.Numerics;
using System.Threading;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Controls;

public class Button : UiElement
{
    [Flags]
    enum ButtonFlags
    {
        Pressed           = 1,
        Hoverable         = 1 << 1,
        Typematic         = 1 << 2,
        DoubleFrame       = 1 << 3,
        AllowDoubleClick  = 1 << 4,
        Hovered           = 1 << 5,
        Clicked           = 1 << 6,
        RightClicked      = 1 << 7,
        ClickTimerPending = 1 << 8,
        SuppressNextDoubleClick = 1 << 9,
    }

    string TimerName => "DoubleClickButton." + _id;

    static int _nextId;
    readonly ButtonFrame _frame;
    readonly int _id;
    float _typematicAccrual;
    ButtonFlags _flags = ButtonFlags.Hoverable;

    public Button(IUiElement content)
    {
        On<HoverEvent>(_ => { IsHovered = true; Hover?.Invoke(); });
        On<BlurEvent>(_ => { IsHovered = false; Blur?.Invoke(); });
        On<UiLeftClickEvent>(OnLeftClick);
        On<UiLeftReleaseEvent>(OnLeftRelease);
        On<UiRightClickEvent>(OnRightClick);
        On<UiRightReleaseEvent>(OnRightRelease);
        On<EngineUpdateEvent>(OnEngineUpdate);

        _id = Interlocked.Increment(ref _nextId);
        _frame = AttachChild(new ButtonFrame(content));
    }

    void OnLeftClick(UiLeftClickEvent e)
    {
        if (!IsHovered) return;

        if (!IsClicked) ButtonDown?.Invoke();

        IsClicked = true;
        if (Typematic) Click?.Invoke();

        e.Propagating = false;
    }

    void OnLeftRelease(UiLeftReleaseEvent _)
    {
        if (!IsClicked) return;
        IsClicked = false;

        if (Typematic)
        {
            _typematicAccrual = 0;
            return;
        }

        if (!IsHovered) return;

        if (DoubleClick == null || !AllowDoubleClick || SuppressNextDoubleClick) // Simple single click only button
        {
            Click?.Invoke();
            SuppressNextDoubleClick = false;
            return;
        }

        if (ClickTimerPending) // If they double-clicked...
        {
            DoubleClick.Invoke();
            ClickTimerPending = false; // Ensure the single-click behaviour doesn't happen.
        }
        else // For the first click, just start the double-click timer.
        {
            RaiseAsync(new WallClockTimerEvent(ReadVar(V.Game.Ui.ButtonDoubleClickIntervalSeconds)))
                .OnCompleted(() =>
                {
                    if (!ClickTimerPending) // They've already double-clicked
                        return;

                    Click?.Invoke();
                    ClickTimerPending = false;
                });

            ClickTimerPending = true;
        }
    }

    void OnRightClick(UiRightClickEvent e)
    {
        if (RightClick == null) return;
        e.Propagating = false;
        IsRightClicked = true;
    }

    void OnRightRelease(UiRightReleaseEvent _)
    {
        if (RightClick == null) return;
        if (IsRightClicked && IsHovered) RightClick.Invoke();
        IsRightClicked = false;
    }

    void OnEngineUpdate(EngineUpdateEvent e)
    {
        if (!Typematic || !IsClicked) return;

        var oldAccrual = _typematicAccrual;
        _typematicAccrual += e.DeltaSeconds;
        var rate = 8 * (int)(2 * oldAccrual);
        var oldAmount = (int)(oldAccrual * rate);
        var newAmount = (int)(_typematicAccrual * rate);
        var delta = newAmount - oldAmount;
        for (int i = 0; i < delta; i++) Click?.Invoke();
    }

    public Button(IText textSource) : this(new UiText(textSource)) { }
    public Button(TextId textId) : this(new StringId(textId)) { }
    public Button(StringId textId) : this((IUiElement)new UiTextBuilder(textId).Center().NoWrap()) { }
    public Button(string literalText) : this((IUiElement)new SimpleText(literalText).Center().NoWrap()) { }
    public Button OnClick(Action callback) { Click += callback; return this; }
    public Button OnRightClick(Action callback) { RightClick += callback; return this; }
    public Button OnDoubleClick(Action callback) { DoubleClick += callback; AllowDoubleClick = true; return this; }
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
    public int Margin { get; set; } = 2;

    ButtonState State => (IsClicked, IsPressed, IsHovered) switch
    {
        (false, false, false) => ButtonState.Normal,
        (false, false, true) => Hoverable ? ButtonState.Hover : ButtonState.Normal,
        (false, true, false) => ButtonState.Pressed,
        (false, true, true) => Hoverable ? ButtonState.HoverPressed : ButtonState.Pressed,
        (true, _, false) => ButtonState.ClickedBlurred,
        (true, _, true) => ButtonState.Clicked,
    };

    public override Vector2 GetSize() => GetMaxChildSize() + new Vector2(2 * Margin, 0);

    protected override int DoLayout<T>(Rectangle extents, int order, T context, LayoutFunc<T> func)
    {
        var innerExtents = new Rectangle(extents.X + Margin, extents.Y, extents.Width - 2 * Margin, extents.Height);
        return base.DoLayout(innerExtents, order, context, func);
    }

    #region Flag Helpers

    void SetFlag(ButtonFlags flag, bool set)
    {
        if (set) _flags |= flag;
        else _flags &= ~flag;
    }

    public bool AllowDoubleClick
    {
        get => 0 != (_flags & ButtonFlags.AllowDoubleClick);
        set => SetFlag(ButtonFlags.AllowDoubleClick, value);
    }

    public bool DoubleFrame
    {
        get => 0 != (_flags & ButtonFlags.DoubleFrame);
        set => SetFlag(ButtonFlags.DoubleFrame, value);
    }

    public bool Typematic
    {
        get => 0 != (_flags & ButtonFlags.Typematic);
        set => SetFlag(ButtonFlags.Typematic, value);
    }

    public bool Hoverable
    {
        get => 0 != (_flags & ButtonFlags.Hoverable);
        set { SetFlag(ButtonFlags.Hoverable, value); _frame.State = State; }
    }

    public bool SuppressNextDoubleClick
    {
        get => 0 != (_flags & ButtonFlags.SuppressNextDoubleClick);
        set { SetFlag(ButtonFlags.SuppressNextDoubleClick, value); _frame.State = State; }
    }

    public bool IsPressed
    {
        get => 0 != (_flags & ButtonFlags.Pressed);
        set { SetFlag(ButtonFlags.Pressed, value); _frame.State = State; }
    }

    bool ClickTimerPending
    {
        get => 0 != (_flags & ButtonFlags.ClickTimerPending);
        set => SetFlag(ButtonFlags.ClickTimerPending, value);
    }

    bool IsHovered
    {
        get => 0 != (_flags & ButtonFlags.Hovered);
        set { SetFlag(ButtonFlags.Hovered, value); _frame.State = State; }
    }

    bool IsClicked
    {
        get => 0 != (_flags & ButtonFlags.Clicked);
        set { SetFlag(ButtonFlags.Clicked, value); _frame.State = State; }
    }

    bool IsRightClicked
    {
        get => 0 != (_flags & ButtonFlags.RightClicked);
        set { SetFlag(ButtonFlags.RightClicked, value); _frame.State = State; }
    }

    #endregion
}