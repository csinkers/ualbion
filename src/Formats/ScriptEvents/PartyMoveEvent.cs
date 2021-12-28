using UAlbion.Api;

namespace UAlbion.Formats.ScriptEvents;

[Event("party_move")] // USED IN SCRIPT
public class PartyMoveEvent : Event, IVerboseEvent
{
    public PartyMoveEvent(int x, int y) { X = x; Y = y; }
    [EventPart("x ")] public int X { get; }
    [EventPart("y")] public int Y { get; }
}