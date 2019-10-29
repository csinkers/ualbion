using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("special2", "Event for ad-hoc debugging / investigation purposes")]
    public class Special2Event : GameEvent
    {
        public Special2Event(float argument) { Argument = argument; }
        [EventPart("argument")]
        public float Argument { get; }
    }
}