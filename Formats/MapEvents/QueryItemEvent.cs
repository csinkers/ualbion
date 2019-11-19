using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class QueryItemEvent : QueryEvent
    {
        public QueryItemEvent(int id, EventType type) : base(id, type) { }
        public ItemId ItemId => (ItemId)Argument-1;
        public override string ToString() => $"query_item {SubType} {ItemId} (method {Unk2})";
    }
}