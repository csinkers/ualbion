using UAlbion.Api;

namespace UAlbion.Core.Events;

public class AddPositionedComponentEvent : Event, IVerboseEvent
{
    public AddPositionedComponentEvent(IPositioned positioned) => Positioned = positioned;
    public IPositioned Positioned { get; }
}