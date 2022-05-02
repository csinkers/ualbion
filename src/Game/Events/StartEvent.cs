using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("start")]
public class StartEvent : GameEvent
{
    public StartEvent(int explosion) { Explosion = explosion; }
    [EventPart("explosion")] public int Explosion { get; }
}