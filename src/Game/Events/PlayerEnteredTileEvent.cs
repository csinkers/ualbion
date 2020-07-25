using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("player_entered_tile")]
    public class PlayerEnteredTileEvent : GameEvent, IVerboseEvent
    {
        public PlayerEnteredTileEvent(int x, int y)
        {
            X = x;
            Y = y;
        }

        [EventPart("x")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }
}
