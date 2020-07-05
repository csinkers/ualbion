using UAlbion.Api;

namespace UAlbion.Core.Events
{
    public class RemovePositionedComponentEvent : Event, IVerboseEvent
    {
        public RemovePositionedComponentEvent(IPositioned positioned) => Positioned = positioned;
        public IPositioned Positioned { get; }
    }
}