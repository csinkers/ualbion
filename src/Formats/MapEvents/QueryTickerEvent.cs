using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("get_ticker")]
public class QueryTickerEvent : QueryEvent
{
    public override QueryType QueryType => QueryType.Ticker;
    [EventPart("ticker")] public TickerId TickerId { get; private set; } // => AssetType == AssetType.Ticker
    [EventPart("op")] public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
    [EventPart("imm")] public byte Immediate { get; private set; } // immediate value?
    QueryTickerEvent() { }
    public QueryTickerEvent(TickerId tickerId, QueryOperation operation, byte immediate)
    {
        TickerId = tickerId;
        Operation = operation;
        Immediate = immediate;
    }
    public static QueryTickerEvent Serdes(QueryTickerEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new QueryTickerEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.TickerId = TickerId.SerdesU16(nameof(TickerId), e.TickerId, mapping, s);
        // field 8 is the next event id when the condition is false and is deserialised as part of the BranchEventNode that this event should belong to.

        s.Assert(zeroes == 0, "QueryTickerEvent: Expected fields 3,4 to be 0");
        return e;
    }
}