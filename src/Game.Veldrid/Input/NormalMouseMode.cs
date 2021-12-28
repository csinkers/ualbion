﻿using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Veldrid.Input;

public class NormalMouseMode : Component
{
    readonly List<Selection> _hits = new();
    readonly UiMouseMoveEvent _moveEvent = new(0, 0);
    readonly UiScrollEvent _scrollEvent = new(0);
    readonly UiLeftClickEvent _leftClickEvent = new();
    readonly UiRightClickEvent _rightClickEvent = new();
    readonly UiLeftReleaseEvent _leftReleaseEvent = new();
    readonly UiRightReleaseEvent _rightReleaseEvent = new();

    Vector2 _lastPosition;

    public NormalMouseMode() => On<InputEvent>(OnInput);
    protected override void Subscribed() => Raise(new SetCursorEvent(Base.CoreSprite.Cursor));

    void OnInput(InputEvent e)
    {
        _hits.Clear();
        Resolve<ISelectionManager>()?.CastRayFromScreenSpace(_hits, e.Snapshot.MousePosition, false, true);

        // Clicks are targeted, releases are broadcast. e.g. if you click and drag a slider and move outside
        // its hover area, then it should switch to "ClickedBlurred". If you then release the button while
        // still outside its hover area and releases were broadcast, it would never receive the release and
        // it wouldn't be able to transition back to Normal
        if (_hits.Count > 0)
        {
            if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Right && x.Down))
                Distribute(_rightClickEvent, _hits, x => x.Target as IComponent);

            if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Left && x.Down))
                Distribute(_leftClickEvent, _hits, x => x.Target as IComponent);

            if ((int)e.Snapshot.WheelDelta != 0)
            {
                _scrollEvent.Delta = (int)e.Snapshot.WheelDelta;
                Distribute(_scrollEvent, _hits, x => x.Target as IComponent);
            }
        }

        if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Left && !x.Down))
            Raise(_leftReleaseEvent);

        if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Right && !x.Down))
            Raise(_rightReleaseEvent);

        if (_lastPosition != e.Snapshot.MousePosition)
        {
            _lastPosition = e.Snapshot.MousePosition;
            var window = Resolve<IWindowManager>();
            var uiPosition = window.NormToUi(window.PixelToNorm(e.Snapshot.MousePosition));
            _moveEvent.X = (int)uiPosition.X;
            _moveEvent.Y = (int)uiPosition.Y;
            Raise(_moveEvent);
        }
    }
}