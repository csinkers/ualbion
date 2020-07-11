using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class QueryItemEvent : MapEvent, IQueryEvent
    {
        QueryItemEvent(QueryType subType)
        {
            QueryType = subType;
        }

        public static QueryItemEvent Serdes(QueryItemEvent e, ISerializer s, QueryType subType)
        {
            e ??= new QueryItemEvent(subType);
            s.Begin();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.ItemId = (ItemId)StoreIncremented.Serdes(nameof(ItemId), (ushort)e.ItemId, s.UInt16);

            ApiUtil.Assert(e.Unk4 == 0);
            ApiUtil.Assert(e.Unk5 == 0);

            s.End();
            return e;
        }

        public QueryOperation Operation { get; private set; }
        public byte Immediate { get; private set; } // immediate value?
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        public ItemId ItemId { get; private set; }

        public override string ToString() => $"query_item {QueryType} {ItemId} {Operation} {Immediate}";
        public override MapEventType EventType => MapEventType.Query;
        public QueryType QueryType { get; }
    }
}
