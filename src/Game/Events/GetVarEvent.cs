using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("get")]
public record GetVarEvent(
    [property: EventPart("name", true)] string Name) : EventRecord;