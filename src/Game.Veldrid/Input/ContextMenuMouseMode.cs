using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Veldrid.Input;

public class ContextMenuMouseMode : Component
{
    readonly List<Selection> _hits = new();
    readonly UiLeftClickEvent _leftClickEvent = new();
    readonly UiLeftReleaseEvent _leftReleaseEvent = new();

    public ContextMenuMouseMode() => On<InputEvent>(OnInput);
    protected override void Subscribed() => Raise(new SetCursorEvent(Base.CoreGfx.Cursor));

    void OnInput(InputEvent e)
    {
        _hits.Clear();
        Resolve<ISelectionManager>()?.CastRayFromScreenSpace(_hits, e.Snapshot.MousePosition, false, true);
        if (_hits.Count > 0 && e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Left && x.Down))
            Distribute(_leftClickEvent, _hits, x => x.Target as IComponent);

        if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Left && !x.Down))
            Raise(_leftReleaseEvent);

        if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Right && x.Down))
            Raise(new CloseWindowEvent());
    }
}