using UAlbion.Config;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.MapEvents
{
    public class EventSource
    {
        public EventSource(AssetId id, TriggerTypes trigger, int x = 0, int y = 0)
        {
            // Trigger = TalkTo for NPC, UseItem for item, Action for event set, Default for none
            Id = id;
            Trigger = trigger;
            X = x;
            Y = y;
        }

        public AssetId Id { get; }
        public TriggerTypes Trigger { get; }
        public int X { get; }
        public int Y { get; }
    }
}
