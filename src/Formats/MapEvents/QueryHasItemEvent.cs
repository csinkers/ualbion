using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

[Event("has_item")]
public class QueryHasItemEvent : QueryEvent
{
    public override QueryType QueryType => QueryType.HasItem;
    [EventPart("op")] public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
    [EventPart("imm")] public byte Immediate { get; private set; } // immediate value?
    [EventPart("item")] public ItemId ItemId { get; private set; } // => AssetType == AssetType.Item
    QueryHasItemEvent() { }
    public QueryHasItemEvent(QueryOperation operation, byte immediate, ItemId itemId)
    {
        Operation = operation;
        Immediate = immediate;
        ItemId = itemId;
    }
    public static QueryHasItemEvent Serdes(QueryHasItemEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new QueryHasItemEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.ItemId = ItemId.SerdesU16(nameof(ItemId), e.ItemId, AssetType.Item, mapping, s);
        // field 8 is the next event id when the condition is false and is deserialised as part of the BranchEventNode that this event should belong to.

        s.Assert(zeroes == 0, "QueryHasItemEvent: Expected fields 3,4 to be 0");
        return e;
    }
}