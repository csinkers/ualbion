using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("play")]
    public class PlayEvent : GameEvent
    {
        public PlayEvent(int unknown) { Unknown = unknown; }
        [EventPart("unknown")] public int Unknown { get; }
    }
}