using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Core.Veldrid.Events;

namespace UAlbion.Game.Veldrid.Input;

public class UiMouseMode : Component
{
    void OnInput(InputEvent e)
    {
        if(e.Snapshot.WheelDelta < 0)
            Raise(new MagnifyEvent(-1));
        if(e.Snapshot.WheelDelta > 0)
            Raise(new MagnifyEvent(1));
    }
}