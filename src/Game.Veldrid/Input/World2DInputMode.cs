using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Veldrid.Input;

public class World2DInputMode : Component
{
    public World2DInputMode() => On<InputEvent>(OnInput);
    protected override void Subscribed() => Raise(new PushMouseModeEvent(MouseMode.Normal));
    protected override void Unsubscribed() => Raise(new PopMouseModeEvent());

    void OnInput(InputEvent e)
    {
        /*
        if(e.Snapshot.WheelDelta < 0)
            Raise(new MagnifyEvent(-1));
        if(e.Snapshot.WheelDelta > 0)
            Raise(new MagnifyEvent(1));
            */
    }
}