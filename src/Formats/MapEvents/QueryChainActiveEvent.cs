using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

[Event("is_chain_active")]
public class QueryChainActiveEvent : QueryEvent
{
    public override QueryType QueryType => QueryType.ChainActive;
    [EventPart("chain")] public byte ChainNum { get; private set; } // immediate value?
    [EventPart("map")] public MapId MapId { get; set; }
    [EventPart("op")] public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
    QueryChainActiveEvent() { }
    public QueryChainActiveEvent(byte chainNum, MapId mapId, QueryOperation operation)
    {
        Operation = operation;
        ChainNum = chainNum;
        MapId = mapId;
    }

    public static QueryChainActiveEvent Serdes(QueryChainActiveEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new QueryChainActiveEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.ChainNum = s.UInt8(nameof(ChainNum), e.ChainNum);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.MapId = MapId.SerdesU16(nameof(MapId), e.MapId, mapping, s);
        // field 8 is the next event id when the condition is false and is deserialised as part of the BranchEventNode that this event should belong to.

        s.Assert(zeroes == 0, "QueryChainActiveEvent: Expected fields 3,4 to be 0");
        return e;
    }
}