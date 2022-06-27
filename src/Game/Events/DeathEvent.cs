using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events;

[Event("death")]
public class DeathEvent : GameEvent
{
    [EventPart("id")] public SheetId Id { get; }
    public DeathEvent(SheetId id) => Id = id;
}