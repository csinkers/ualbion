using UAlbion.Api;

namespace UAlbion.Core.Events;

public class PositionedComponentMovedEvent : Event, IVerboseEvent
{
    public PositionedComponentMovedEvent(IPositioned positioned) => Positioned = positioned;
    public IPositioned Positioned { get; }
}