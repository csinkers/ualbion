using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

public class PositionedComponentMovedEvent : Event, IVerboseEvent
{
    public PositionedComponentMovedEvent(IPositioned positioned) => Positioned = positioned;
    public IPositioned Positioned { get; }
}