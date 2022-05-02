using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

public class RemovePositionedComponentEvent : Event, IVerboseEvent
{
    public RemovePositionedComponentEvent(IPositioned positioned) => Positioned = positioned;
    public IPositioned Positioned { get; }
}