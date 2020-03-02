using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class QueryItemEvent : Event, IQueryEvent
    {
        QueryItemEvent(QueryType subType)
        {
            QueryType = subType;
        }

        public static QueryItemEvent Serdes(QueryItemEvent e, ISerializer s, QueryType subType)
        {
            e ??= new QueryItemEvent(subType);
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.ItemId = (ItemId)StoreIncremented.Serdes(nameof(ItemId), (ushort)e.ItemId, s.UInt16);
            e.FalseEventId = ConvertMaxToNull.Serdes(nameof(FalseEventId), e.FalseEventId, s.UInt16);

            ApiUtil.Assert(e.Unk4 == 0);
            ApiUtil.Assert(e.Unk5 == 0);

            return e;
        }

        public QueryOperation Operation { get; private set; }
        public byte Immediate { get; private set; } // immediate value?
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        public ItemId ItemId { get; set; }//=> (ItemId)Argument-1;
        public ushort? FalseEventId { get; set; }

        public override string ToString() => $"query_item {QueryType} {ItemId} {Operation} {Immediate}";
        public MapEventType EventType => MapEventType.Query;
        public QueryType QueryType { get; }
    }
}
