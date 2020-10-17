using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events
{
    [Event("new_game", "Start a new game")]
    public class NewGameEvent : GameEvent
    {
        public NewGameEvent(MapId mapId, ushort x, ushort y)
        {
            MapId = mapId;
            X = x;
            Y = y;
        }

        [EventPart("mapId")] public MapId MapId { get; }
        [EventPart("x")] public ushort X { get; }
        [EventPart("y")] public ushort Y { get; }
    }
}
