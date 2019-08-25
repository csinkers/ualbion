using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("song")]
    public class SongEvent : GameEvent
    {
        public SongEvent(int songId) { SongId = songId; }
        [EventPart("songId")] public int SongId { get; }
    }
}