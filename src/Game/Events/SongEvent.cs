using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events
{
    [Event("song")] // USED IN SCRIPT
    public class SongEvent : GameEvent
    {
        public SongEvent(SongId songId) { SongId = songId; }
        [EventPart("songId")] public SongId SongId { get; }
    }
}
