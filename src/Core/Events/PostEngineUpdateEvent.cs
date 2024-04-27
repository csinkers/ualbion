using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("post_update")]
public class PostEngineUpdateEvent : EngineEvent, IVerboseEvent
{
}