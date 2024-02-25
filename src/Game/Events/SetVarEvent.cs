using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("set")]
public record SetVarEvent(
    [property: EventPart("name")] string Name,
    [property: EventPart("value")] string Value) : EventRecord;