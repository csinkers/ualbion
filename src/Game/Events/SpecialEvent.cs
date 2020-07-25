using UAlbion.Api;
using UAlbion.Core;

namespace UAlbion.Game.Events
{
    [Event("special", "Event for ad-hoc debugging / investigation purposes")]
    public class SpecialEvent : GameEvent
    {
        public SpecialEvent(ValueOperation operation, float argument)
        {
            Operation = operation;
            Argument = argument;
        }

        [EventPart("operation", "Valid values: set, add, mult")]
        public ValueOperation Operation { get; }

        [EventPart("argument")]
        public float Argument { get; }
    }
}
