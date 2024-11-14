using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Veldrid.Input;

public class ExclusiveMouseMode : Component
{
    readonly List<Selection> _hits = [];
    public ExclusiveMouseMode() => On<MouseInputEvent>(OnInput);

    void OnInput(MouseInputEvent e)
    {
        _hits.Clear();
        Resolve<ISelectionManager>()?.CastRayFromScreenSpace(_hits, e.MousePosition, false, true);

        if (e.CheckMouse(MouseButton.Left, false))
        {
            Raise(new UiLeftReleaseEvent());
            Raise(new MouseModeEvent(MouseMode.Normal));
        }

        if (e.CheckMouse(MouseButton.Right, false))
        {
            Raise(new UiRightReleaseEvent());
            Raise(new MouseModeEvent(MouseMode.Normal));
        }
    }
}