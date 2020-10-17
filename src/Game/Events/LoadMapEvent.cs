using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events
{
    [Event("load_map")]
    public class LoadMapEvent : GameEvent
    {
        public LoadMapEvent(MapId mapId) { MapId = mapId; }
        [EventPart("id")] public MapId MapId { get; }
    }
}
