using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("fast_clock")]
    public class FastClockEvent : GameEvent, IVerboseEvent
    {
        public FastClockEvent(int frames) { Frames = frames; }
        [EventPart("frames")] public int Frames { get; }
    }
}
