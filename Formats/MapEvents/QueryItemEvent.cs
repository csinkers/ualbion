using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class QueryItemEvent : IQueryEvent
    {
        QueryItemEvent(QueryType subType)
        {
            QueryType = subType;
        }

        public static QueryItemEvent Translate(QueryItemEvent e, ISerializer s, QueryType subType)
        {
            e ??= new QueryItemEvent(subType);
            s.Dynamic(e, nameof(Unk2));
            s.Dynamic(e, nameof(Unk3));
            s.Dynamic(e, nameof(Unk4));
            s.Dynamic(e, nameof(Unk5));
            s.UInt16(nameof(ItemId),
                () => (ushort)(e.ItemId + 1),
                x => e.ItemId = (ItemId)(x - 1));

            s.UInt16(nameof(FalseEventId),
                () => e.FalseEventId ?? 0xffff,
                x => e.FalseEventId = x == 0xffff ? (ushort?)null : x);

            return e;
        }

        public byte Unk2 { get; private set; } // method to use for check? 0,1,2,3,4,5
        public byte Unk3 { get; private set; } // immediate value?
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        public ItemId ItemId { get; set; }//=> (ItemId)Argument-1;
        public ushort? FalseEventId { get; set; }

        public override string ToString() => $"query_item {QueryType} {ItemId} (method {Unk2})";
        public MapEventType EventType => MapEventType.Query;
        public QueryType QueryType { get; }
    }
}
