using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.ScriptEvents;

[Event("ambient")] // USED IN SCRIPT
public class AmbientEvent : Event
{
    public AmbientEvent(SongId songId) { SongId = songId; }
    [EventPart("songId")] public SongId SongId { get; }
}