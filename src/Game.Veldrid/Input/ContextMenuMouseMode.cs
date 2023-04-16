using System.Collections.Generic;
using Veldrid;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Game.Events;

namespace UAlbion.Game.Veldrid.Input;

public class ContextMenuMouseMode : Component
{
    readonly List<Selection> _hits = new();
    readonly UiLeftClickEvent _leftClickEvent = new();
    readonly UiLeftReleaseEvent _leftReleaseEvent = new();
    bool _wasClockRunning;

    public ContextMenuMouseMode() => On<MouseInputEvent>(OnInput);
    protected override void Subscribed()
    {
        Raise(new SetCursorEvent(Base.CoreGfx.Cursor));
        _wasClockRunning = Resolve<IClock>()?.IsRunning ?? false;
        if(_wasClockRunning)
            Raise(new StopClockEvent());
    }

    protected override void Unsubscribed()
    {
        if (_wasClockRunning)
            Raise(new StartClockEvent());
    }

    void OnInput(MouseInputEvent e)
    {
        _hits.Clear();
        Resolve<ISelectionManager>()?.CastRayFromScreenSpace(_hits, e.MousePosition, false, true);
        if (_hits.Count > 0 && e.CheckMouse(MouseButton.Left, true))
            Distribute(_leftClickEvent, _hits, x => x.Target as IComponent);

        if (e.CheckMouse(MouseButton.Left, false))
            Raise(_leftReleaseEvent);

        if (e.CheckMouse(MouseButton.Right, true))
            Raise(new CloseWindowEvent());
    }
}