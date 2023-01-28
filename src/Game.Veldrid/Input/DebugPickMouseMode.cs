using System.Collections.Generic;
using Veldrid;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;

namespace UAlbion.Game.Veldrid.Input;

public class DebugPickMouseMode : Component
{
    readonly List<Selection> _hits = new();
    readonly PopMouseModeEvent _popMouseModeEvent = new();

    public DebugPickMouseMode() => On<MouseInputEvent>(OnInput);

    void OnInput(MouseInputEvent e)
    {
        if (e.Snapshot.CheckMouse(MouseButton.Left, true))
        {
            var layoutManager = TryResolve<ILayoutManager>();
            layoutManager?.RequestSnapshot();
            Raise(_popMouseModeEvent);
            return;
        }

        _hits.Clear();
        Resolve<ISelectionManager>()?.CastRayFromScreenSpace(_hits, e.Snapshot.MousePosition, true, true);
        Raise(new ShowDebugInfoEvent(_hits, e.Snapshot.MousePosition));
    }
}