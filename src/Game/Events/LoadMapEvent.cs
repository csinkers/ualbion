using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Events;

[Event("load_map", null, "lm")]
public class LoadMapEvent : GameEvent
{
    public LoadMapEvent(MapId mapId) { MapId = mapId; }
    [EventPart("id")] public MapId MapId { get; }
}