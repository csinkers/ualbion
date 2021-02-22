using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("play")] // USED IN SCRIPT
    public class PlayEvent : GameEvent
    {
        public PlayEvent(int unknown) { Unknown = unknown; }
        [EventPart("unknown")] public int Unknown { get; }
    }
}
