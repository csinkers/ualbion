using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets.Maps;

#pragma warning disable CA1034 // Nested types should not be visible
namespace UAlbion.Formats.MapEvents
{
    public abstract class EventSource
    {
        public class None : EventSource
        {
            public override TriggerTypes Trigger => TriggerTypes.Default;
            public override string ToString() => "Ø";
        }
        public class Map : EventSource
        {
            public Map(MapDataId mapId, TriggerTypes trigger, int x, int y)
            {
                MapId = mapId;
                Trigger = trigger;
                X = x;
                Y = y;
            }

            public MapDataId MapId { get; }
            public override TriggerTypes Trigger { get; }
            public int X { get; }
            public int Y { get; }
            public override string ToString() => $"Map{(int)MapId}:{Trigger}:{X}:{Y}";
        }

        public class EventSet : EventSource
        {
            public EventSetId EventSetId { get; }
            public EventSet(EventSetId id) => EventSetId = id;
            public override TriggerTypes Trigger => TriggerTypes.Action;
            public override string ToString() => $"ES:{EventSetId}";
        }

        public class Npc : EventSource
        {
            public Npc(NpcCharacterId npcId) => NpcId = npcId;
            public NpcCharacterId NpcId { get; }
            public override string ToString() => $"Npc:{NpcId}";
            public override TriggerTypes Trigger => TriggerTypes.TalkTo;
        }

        public class Item : EventSource
        {
            public Item(ItemId usedItem) => ItemId = usedItem;
            public ItemId ItemId { get; }
            public override string ToString() => $"Item:{ItemId}";
            public override TriggerTypes Trigger => TriggerTypes.UseItem;
        }

        public abstract TriggerTypes Trigger { get; }
    }
}
#pragma warning restore CA1034 // Nested types should not be visible
