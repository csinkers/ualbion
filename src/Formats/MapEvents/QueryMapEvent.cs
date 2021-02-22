using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("query_map")]
    public class QueryMapEvent : QueryEvent
    {
        public override QueryType QueryType => QueryType.Map;
        [EventPart("op")] public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
        [EventPart("imm")] public byte Immediate { get; private set; } // immediate value?
        [EventPart("map")] public MapId MapId { get; private set; } // => AssetType == AssetType.Map
        QueryMapEvent() { }
        public QueryMapEvent(QueryOperation operation, byte immediate, MapId mapId)
        {
            Operation = operation;
            Immediate = immediate;
            MapId = mapId;
        }
        public static QueryMapEvent Serdes(QueryMapEvent e, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new QueryMapEvent();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
            int zeroes = s.UInt8(null, 0);
            zeroes += s.UInt8(null, 0);
            e.MapId = MapId.SerdesU16(nameof(MapId), e.MapId, mapping, s);
            // field 8 is the next event id when the condition is and is deserialised as part of the BranchEventNode that this event should belong to.

            s.Assert(zeroes == 0, "QueryMapEvent: Expected fields 3,4 to be 0");
            return e;
        }
    }
}