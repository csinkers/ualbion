using UAlbion.Api;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Events
{
    [Event("party_turn")]
    public class PartyTurnEvent : GameEvent
    {
        public PartyTurnEvent(TeleportDirection direction) { Direction = direction; }
        [EventPart("direction")] public TeleportDirection Direction { get; }
    }
}