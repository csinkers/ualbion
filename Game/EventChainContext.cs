using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game
{
    public class EventChainContext
    {
        IEventNode _node;
        public TriggerType Trigger { get; set; }
        public EventChain Chain { get; set; }

        public IEventNode Node
        {
            get => _node;
            set { LastNode = _node; _node = value; }
        }
        public IEventNode LastNode { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public ItemId? UsedItem { get; set; }
        public bool ClockWasRunning { get; set; }
        public bool LastEventResult { get; set; }

        public EventChainContext Clone() => (EventChainContext)MemberwiseClone();
    }
}