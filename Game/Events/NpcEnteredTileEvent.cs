using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("npc_entered_tile")]
    public class NpcEnteredTileEvent : GameEvent, IVerboseEvent
    {
        public NpcEnteredTileEvent(NpcCharacterId id, int x, int y)
        {
            Id = id;
            X = x;
            Y = y;
        }

        [EventPart("id")] public NpcCharacterId Id { get; }
        [EventPart("x")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }
}