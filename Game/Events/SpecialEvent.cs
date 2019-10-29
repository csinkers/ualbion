using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("special", "Event for ad-hoc debugging / investigation purposes")]
    public class SpecialEvent : GameEvent
    {
        public SpecialEvent(float argument) { Argument = argument; }
        [EventPart("argument")]
        public float Argument { get; }
    }
}