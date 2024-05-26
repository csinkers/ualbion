using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("is_npc_active_on_map")]
public class QueryNpcActiveOnMapEvent : QueryEvent
{
    public override QueryType QueryType => QueryType.NpcActiveOnMap;
    [EventPart("op")] public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
    [EventPart("npc")] public byte NpcNum { get; private set; } // immediate value?
    [EventPart("map")] public MapId MapId { get; set; }
    QueryNpcActiveOnMapEvent() { }
    public QueryNpcActiveOnMapEvent(QueryOperation operation, byte npcNum, MapId mapId)
    {
        Operation = operation;
        NpcNum = npcNum;
        MapId = mapId;
    }
    public static QueryNpcActiveOnMapEvent Serdes(QueryNpcActiveOnMapEvent e, AssetMapping mapping, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new QueryNpcActiveOnMapEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.NpcNum = s.UInt8(nameof(NpcNum), e.NpcNum);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.MapId = MapId.SerdesU16(nameof(MapId), e.MapId, mapping, s);
        // field 8 is the next event id when the condition is false and is deserialised as part of the BranchEventNode that this event should belong to.

        s.Assert(zeroes == 0, "QueryNpcActiveOnMapEvent: Expected fields 3,4 to be 0");
        return e;
    }
}