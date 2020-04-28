using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets.Map;

namespace UAlbion.Formats.MapEvents
{
    public abstract class EventSource
    {
        public class None : EventSource
        {
            public override TriggerType Trigger => TriggerType.Default;

            public None() : base(AssetType.MapText, 0) { }
        }
        public class Map : EventSource
        {
            public Map(MapDataId mapId, TriggerType trigger, int x, int y) : base(AssetType.MapText, (int)mapId)
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
            public EventSet(EventSetId id) : base(AssetType.EventText, (int)id) { }
            public override TriggerType Trigger => TriggerType.Action;
            public override string ToString() => $"ES{Id}";
        }

        public class Npc : EventSource
        {
            public Npc(MapDataId mapId, NpcCharacterId npcId) : base(AssetType.MapText, (int)mapId) => NpcId = npcId;
            public NpcCharacterId NpcId { get; }
            public override string ToString() => $"Npc:M{Id}:{NpcId}";
            public override TriggerType Trigger => TriggerType.TalkTo;
        }

        public class Item : EventSource
        {
            public Item(MapDataId mapId, ItemId usedItem) : base(AssetType.MapText, (int)mapId) => ItemId = usedItem;
            public ItemId ItemId { get; }
            public override string ToString() => $"Item:M{Id}:{ItemId}";
            public override TriggerType Trigger => TriggerType.UseItem;
        }

        public abstract TriggerType Trigger { get; }
        public AssetType Type { get; }
        public int Id { get; }

        public EventSource(AssetType type, int id)
        {
            Type = type;
            Id = id;
        }
    }
}