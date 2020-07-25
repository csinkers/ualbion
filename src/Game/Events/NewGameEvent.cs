using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("new_game", "Start a new game")]
    public class NewGameEvent : GameEvent
    {
        public NewGameEvent(MapDataId mapId, ushort x, ushort y)
        {
            MapId = mapId;
            X = x;
            Y = y;
        }

        [EventPart("mapId")] public MapDataId MapId { get; }
        [EventPart("x")] public ushort X { get; }
        [EventPart("y")] public ushort Y { get; }
    }
}
