using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game
{
    public class EventChainContext
    {
        public TriggerType Trigger { get; set; }
        public IEventNode Node { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public ItemId? UsedItem { get; set; }
        public bool ClockWasRunning { get; set; }
        public bool LastEventResult { get; set; }
    }
}