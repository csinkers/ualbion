using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("update")]
    public class UpdateEvent : GameEvent, IVerboseEvent
    {
        public UpdateEvent(int frames) { Frames = frames; }
        [EventPart("frames")] public int Frames { get; }
    }
}
