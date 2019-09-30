using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("song")]
    public class SongEvent : GameEvent
    {
        public SongEvent(SongId songId) { SongId = songId; }
        [EventPart("songId")] public SongId SongId { get; }
    }
}