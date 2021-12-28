using UAlbion.Api;

namespace UAlbion.Formats.ScriptEvents;

[Event("party_jump")] // USED IN SCRIPT
public class PartyJumpEvent : Event
{
    public PartyJumpEvent(int x, int y) { X = x; Y = y; }
    [EventPart("x ")] public int X { get; }
    [EventPart("y")] public int Y { get; }
}