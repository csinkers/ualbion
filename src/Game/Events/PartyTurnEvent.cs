using UAlbion.Api;
using UAlbion.Formats;

namespace UAlbion.Game.Events
{
    [Event("party_turn")] // USED IN SCRIPT
    public class PartyTurnEvent : GameEvent
    {
        public PartyTurnEvent(Direction direction) { Direction = direction; }
        [EventPart("direction")] public Direction Direction { get; }
    }
}
