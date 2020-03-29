using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    public class EventContext
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
        public NpcCharacterId? NpcId { get; set; }
        public ItemId? UsedItem { get; set; }
        public bool ClockWasRunning { get; set; }
        public bool LastEventResult { get; set; }

        public EventContext Clone() => (EventContext)MemberwiseClone();
    }
}
