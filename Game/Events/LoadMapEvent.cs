using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("load_map")]
    public class LoadMapEvent : GameEvent
    {
        public LoadMapEvent(MapDataId mapId) { MapId = mapId; }
        [EventPart("id")] public MapDataId MapId { get; }
    }
}
