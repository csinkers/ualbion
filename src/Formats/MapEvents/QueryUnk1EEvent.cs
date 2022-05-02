using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

[Event("query_unk1e")]
public class QueryUnk1EEvent : QueryEvent
{
    public override QueryType QueryType => QueryType.Unk1E;
    [EventPart("op")] public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
    [EventPart("imm")] public byte Immediate { get; private set; } // immediate value?
    [EventPart("arg")] public ushort Argument { get; set; }
    QueryUnk1EEvent() { }
    public QueryUnk1EEvent(QueryOperation operation, byte immediate, ushort argument)
    {
        Operation = operation;
        Immediate = immediate;
        Argument = argument;
    }
    public static QueryUnk1EEvent Serdes(QueryUnk1EEvent e, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new QueryUnk1EEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.Argument = s.UInt16(nameof(Argument), e.Argument);
        // field 8 is the next event id when the condition is false and is deserialised as part of the BranchEventNode that this event should belong to.

        s.Assert(zeroes == 0, "QueryUnk1EEvent: Expected fields 3,4 to be 0");
        return e;
    }
}