using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.ScriptEvents;

[Event("play")] // USED IN SCRIPT
public class PlayEvent : Event
{
    public PlayEvent(int unknown) { Unknown = unknown; }
    [EventPart("unknown")] public int Unknown { get; }
}