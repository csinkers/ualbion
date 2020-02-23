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

        public static QueryItemEvent Serdes(QueryItemEvent e, ISerializer s, QueryType subType)
        {
            e ??= new QueryItemEvent(subType);
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.ItemId = (ItemId)StoreIncremented.Serdes(nameof(ItemId), (ushort)e.ItemId, s.UInt16);
            e.FalseEventId = ConvertMaxToNull.Serdes(nameof(FalseEventId), e.FalseEventId, s.UInt16);
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
