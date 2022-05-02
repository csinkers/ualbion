using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("post_update")]
public class PostGameUpdateEvent : GameEvent, IVerboseEvent
{
}