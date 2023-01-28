using UAlbion.Api.Eventing;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Veldrid.Input;

public class World2DInputMode : Component
{
    protected override void Subscribed() => Raise(new PushMouseModeEvent(MouseMode.Normal));
    protected override void Unsubscribed() => Raise(new PopMouseModeEvent());
}