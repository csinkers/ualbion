using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("pause")]
    public class PauseEvent : GameEvent
    {
        public PauseEvent(int frames) { Frames = frames; }
        [EventPart("frames")] public int Frames { get; }
    }
}