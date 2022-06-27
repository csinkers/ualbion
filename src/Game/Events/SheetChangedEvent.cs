using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events;

[Event("sheet:updated")]
public class SheetChangedEvent : GameEvent, IVerboseEvent
{
    public SheetChangedEvent(SheetId id) => Id = id;
    [EventPart("id")] public SheetId Id { get; }
}