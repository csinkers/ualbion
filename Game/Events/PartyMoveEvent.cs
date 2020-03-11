using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("party_move")]
    public class PartyMoveEvent : GameEvent, IVerboseEvent
    {
        public PartyMoveEvent(int x, int y) { X = x; Y = y; }
        [EventPart("x ")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }
}
