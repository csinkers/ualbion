using System.Collections.Generic;
using System.Linq;
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
    readonly List<Selection> _hits = new();
    public ExclusiveMouseMode() => On<InputEvent>(OnInput);

    void OnInput(InputEvent e)
    {
        _hits.Clear();
        Resolve<ISelectionManager>()?.CastRayFromScreenSpace(_hits, e.Snapshot.MousePosition, false, true);

        if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Left && !x.Down))
        {
            Raise(new UiLeftReleaseEvent());
            Raise(new MouseModeEvent(MouseMode.Normal));
        }

        if (e.Snapshot.MouseEvents.Any(x => x.MouseButton == MouseButton.Right && !x.Down))
        {
            Raise(new UiRightReleaseEvent());
            Raise(new MouseModeEvent(MouseMode.Normal));
        }
    }
}