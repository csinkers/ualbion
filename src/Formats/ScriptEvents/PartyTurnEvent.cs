using UAlbion.Api;

namespace UAlbion.Formats.ScriptEvents;

[Event("party_turn")] // USED IN SCRIPT
public class PartyTurnEvent : Event
{
    public PartyTurnEvent(Direction direction) { Direction = direction; }
    [EventPart("direction")] public Direction Direction { get; }
}