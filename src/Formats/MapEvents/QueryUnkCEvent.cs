using System;
using SerdesNet;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

[Event("query_unkc")]
public class QueryUnkCEvent : QueryEvent
{
    public override QueryType QueryType => QueryType.UnkC;
    [EventPart("op")] public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5z
    [EventPart("imm")] public byte Immediate { get; private set; } // immediate value?
    [EventPart("arg")] public ushort Argument { get; set; }
    QueryUnkCEvent() { }
    public QueryUnkCEvent(QueryOperation operation, byte immediate, ushort argument)
    {
        Operation = operation;
        Immediate = immediate;
        Argument = argument;
    }
    public static QueryUnkCEvent Serdes(QueryUnkCEvent e, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new QueryUnkCEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.Argument = s.UInt16(nameof(Argument), e.Argument);
        // field 8 is the next event id when the condition is false and is deserialised as part of the BranchEventNode that this event should belong to.

        s.Assert(zeroes == 0, "QueryUnkCEvent: Expected fields 3,4 to be 0");
        return e;
    }
}