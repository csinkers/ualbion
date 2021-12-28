using System;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents;

[Event("result")]
public class QueryPreviousActionResultEvent : QueryEvent
{
    public override QueryType QueryType => QueryType.PreviousActionResult;
    [EventPart("op", true, QueryOperation.IsTrue)] public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
    [EventPart("imm", true, (byte)0)] public byte Immediate { get; private set; } // immediate value?
    [EventPart("arg", true, (ushort)0)] public ushort Argument { get; set; }
    QueryPreviousActionResultEvent() { }
    public QueryPreviousActionResultEvent(QueryOperation operation, byte immediate, ushort argument)
    {
        Operation = operation;
        Immediate = immediate;
        Argument = argument;
    }
    public static QueryPreviousActionResultEvent Serdes(QueryPreviousActionResultEvent e, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new QueryPreviousActionResultEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.Argument = s.UInt16(nameof(Argument), e.Argument);
        // field 8 is the next event id when the condition is false and is deserialised as part of the BranchEventNode that this event should belong to.

        s.Assert(zeroes == 0, "QueryPreviousActionResultEvent: Expected fields 3,4 to be 0");
        return e;
    }
}