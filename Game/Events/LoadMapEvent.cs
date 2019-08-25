using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("load_map")]
    public class LoadMapEvent : GameEvent
    {
        public LoadMapEvent(int mapId) { MapId = mapId; }
        [EventPart(("id"))] public int MapId { get; }
    }
}