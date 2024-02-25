using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("reset")]
public record ResetVarEvent(
    [property: EventPart("name")] string Name) : EventRecord;