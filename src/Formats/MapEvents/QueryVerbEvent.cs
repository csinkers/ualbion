using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.MapEvents;

[Event("verb")]
public class QueryVerbEvent : QueryEvent
{
    public override QueryType QueryType => QueryType.ChosenVerb;
    [EventPart("arg")] public TriggerType TriggerType { get; set; }
    [EventPart("op", true, QueryOperation.NonZero)] public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
    [EventPart("imm", true, (byte)0)] public byte Immediate { get; private set; } // immediate value?
    QueryVerbEvent() { }
    public QueryVerbEvent(TriggerType triggerType, QueryOperation operation, byte immediate)
    {
        TriggerType = triggerType;
        Operation = operation;
        Immediate = immediate;
    }
    public static QueryVerbEvent Serdes(QueryVerbEvent e, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new QueryVerbEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.TriggerType = s.EnumU16(nameof(TriggerType), e.TriggerType);
        // field 8 is the next event id when the condition is false and is deserialised as part of the BranchEventNode that this event should belong to.

        s.Assert(zeroes == 0, "QueryChosenVerbEvent: Expected fields 3,4 to be 0");
        return e;
    }
}