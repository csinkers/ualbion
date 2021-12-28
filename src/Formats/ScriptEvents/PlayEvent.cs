using UAlbion.Api;

namespace UAlbion.Formats.ScriptEvents;

[Event("play")] // USED IN SCRIPT
public class PlayEvent : Event
{
    public PlayEvent(int unknown) { Unknown = unknown; }
    [EventPart("unknown")] public int Unknown { get; }
}