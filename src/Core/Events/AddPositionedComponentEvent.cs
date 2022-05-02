using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

public class AddPositionedComponentEvent : Event, IVerboseEvent
{
    public AddPositionedComponentEvent(IPositioned positioned) => Positioned = positioned;
    public IPositioned Positioned { get; }
}