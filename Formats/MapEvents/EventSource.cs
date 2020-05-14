using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets.Map;

namespace UAlbion.Formats.MapEvents
{
    public abstract class EventSource
    {
        public class None : EventSource
        {
            public override TriggerType Trigger => TriggerType.Default;
            public override string ToString() => "Ø";
        }
        public class Map : EventSource
        {
            public Map(MapDataId mapId, TriggerType trigger, int x, int y)
            {
                MapId = mapId;
                Trigger = trigger;
                X = x;
                Y = y;
            }

            public MapDataId MapId { get; }
            public override TriggerType Trigger { get; }
            public int X { get; }
            public int Y { get; }
            public override string ToString() => $"Map{(int)MapId}:{Trigger}:{X}:{Y}";
        }

        public class EventSet : EventSource
        {
            public EventSetId EventSetId { get; }
            public EventSet(EventSetId id) => EventSetId = id;
            public override TriggerType Trigger => TriggerType.Action;
            public override string ToString() => $"ES:{EventSetId}";
        }

        public class Npc : EventSource
        {
            public Npc(NpcCharacterId npcId) => NpcId = npcId;
            public NpcCharacterId NpcId { get; }
            public override string ToString() => $"Npc:{NpcId}";
            public override TriggerType Trigger => TriggerType.TalkTo;
        }

        public class Item : EventSource
        {
            public Item(ItemId usedItem) => ItemId = usedItem;
            public ItemId ItemId { get; }
            public override string ToString() => $"Item:{ItemId}";
            public override TriggerType Trigger => TriggerType.UseItem;
        }

        public abstract TriggerType Trigger { get; }
    }
}