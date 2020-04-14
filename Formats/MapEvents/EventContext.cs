using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    public class EventContext
    {
        IEventNode _node;

        public EventContext(EventSource source)
        {
            Source = source;
        }

        public EventChain Chain { get; set; }
        public IEventNode Node { get => _node; set { LastNode = _node; _node = value; } }
        public IEventNode LastNode { get; set; }
        public EventSource Source { get; }
        public bool ClockWasRunning { get; set; }
        public bool LastEventResult { get; set; }

        public EventContext Clone() => (EventContext)MemberwiseClone();
    }
}
