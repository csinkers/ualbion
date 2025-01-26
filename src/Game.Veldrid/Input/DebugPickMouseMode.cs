using System.Collections.Generic;
using Veldrid.Sdl2;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Game.Events;
using UAlbion.Game.Gui;

namespace UAlbion.Game.Veldrid.Input;

public class DebugPickMouseMode : Component
{
    readonly List<Selection> _hits = [];
    readonly PopMouseModeEvent _popMouseModeEvent = new();
    readonly InspectorPickEvent _pickEvent;

    public DebugPickMouseMode()
    {
        _pickEvent = new InspectorPickEvent(_hits);
        On<MouseInputEvent>(OnInput);
    }

    void OnInput(MouseInputEvent e)
    {
        if (e.CheckMouse(MouseButton.Left, true))
        {
            var layoutManager = TryResolve<ILayoutManager>();
            layoutManager?.RequestSnapshot();
            Raise(_popMouseModeEvent);
            return;
        }

        _hits.Clear();
        Resolve<ISelectionManager>()?.CastRayFromScreenSpace(
            _hits,
            e.MousePosition,
            true,
            true);

        Raise(_pickEvent);
    }
}
