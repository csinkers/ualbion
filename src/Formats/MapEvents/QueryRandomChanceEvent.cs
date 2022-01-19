using System;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents;

[Event("random_chance")]
public class QueryRandomChanceEvent : QueryEvent
{
    public override QueryType QueryType => QueryType.RandomChance;
    [EventPart("arg")] public ushort Argument { get; set; }
    [EventPart("op")] public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
    [EventPart("imm")] public byte Immediate { get; private set; } // immediate value?
    QueryRandomChanceEvent() { }
    public QueryRandomChanceEvent(ushort argument, QueryOperation operation, byte immediate)
    {
        // argument compared against random number in range [0..100]
        Operation = operation;
        Immediate = immediate;
        Argument = argument;
    }
    public static QueryRandomChanceEvent Serdes(QueryRandomChanceEvent e, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new QueryRandomChanceEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.Argument = s.UInt16(nameof(Argument), e.Argument);
        // field 8 is the next event id when the condition is false and is deserialised as part of the BranchEventNode that this event should belong to.

        s.Assert(zeroes == 0, "QueryRandomChanceEvent: Expected fields 3,4 to be 0");
        return e;
    }
}