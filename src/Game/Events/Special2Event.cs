using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core;

namespace UAlbion.Game.Events;

[Event("special2", "Event for ad-hoc debugging / investigation purposes")]
public class Special2Event : GameEvent
{
    public Special2Event(ValueOperation operation, float argument)
    {
        Operation = operation;
        Argument = argument;
    }

    [EventPart("operation", "Valid values: set, add, mult")]
    public ValueOperation Operation { get; }

    [EventPart("argument")]
    public float Argument { get; }
}