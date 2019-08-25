using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("party_turn")]
    public class PartyTurnEvent : GameEvent
    {
        public PartyTurnEvent(int direction) { Direction = direction; }
        [EventPart("direction")] public int Direction { get; }
    }
}