using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("party_jump")] // USED IN SCRIPT
    public class PartyJumpEvent : GameEvent
    {
        public PartyJumpEvent(int x, int y) { X = x; Y = y; }
        [EventPart("x ")] public int X { get; }
        [EventPart("y")] public int Y { get; }
    }
}
