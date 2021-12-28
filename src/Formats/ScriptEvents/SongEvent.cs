using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.ScriptEvents;

[Event("song")] // USED IN SCRIPT
public class SongEvent : Event
{
    public SongEvent(SongId songId) { SongId = songId; }
    [EventPart("songId")] public SongId SongId { get; }
}