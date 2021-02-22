using System;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    [Event("query_unk4")]
    public class QueryUnk4Event : QueryEvent
    {
        public override QueryType QueryType => QueryType.Unk4;
        [EventPart("op")] public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
        [EventPart("imm")] public byte Immediate { get; private set; } // immediate value?
        [EventPart("arg")] public ushort Argument { get; set; }
        QueryUnk4Event() { }
        public QueryUnk4Event(QueryOperation operation, byte immediate, ushort argument)
        {
            Operation = operation;
            Immediate = immediate;
            Argument = argument;
        }
        public static QueryUnk4Event Serdes(QueryUnk4Event e, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new QueryUnk4Event();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
            int zeroes = s.UInt8(null, 0);
            zeroes += s.UInt8(null, 0);
            e.Argument = s.UInt16(nameof(Argument), e.Argument);
            // field 8 is the next event id when the condition is and is deserialised as part of the BranchEventNode that this event should belong to.

            s.Assert(zeroes == 0, "QueryUnk4Event: Expected fields 3,4 to be 0");
            return e;
        }
    }
}