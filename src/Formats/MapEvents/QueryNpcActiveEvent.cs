using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;

namespace UAlbion.Formats.MapEvents;

[Event("is_npc_active")]
public class QueryNpcActiveEvent : QueryEvent
{
    public override QueryType QueryType => QueryType.NpcActive;
    [EventPart("npcNum")] public ushort NpcNum { get; set; }
    [EventPart("op", true, QueryOperation.AlwaysFalse)] public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
    [EventPart("imm", true, (byte)0)] public byte Immediate { get; private set; } // immediate value?
    QueryNpcActiveEvent() { }
    public QueryNpcActiveEvent(ushort npcNum, QueryOperation operation, byte immediate)
    {
        Immediate = immediate;
        NpcNum = npcNum;
        Operation = operation;
    }

    public static QueryNpcActiveEvent Serdes(QueryNpcActiveEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new QueryNpcActiveEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.NpcNum = s.UInt16(nameof(NpcNum), e.NpcNum);
        // field 8 is the next event id when the condition is false and is deserialised as part of the BranchEventNode that this event should belong to.

        s.Assert(zeroes == 0, "QueryNpcActiveEvent: Expected fields 3,4 to be 0");
        return e;
    }
}