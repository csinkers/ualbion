using System;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents;

[Event("is_debug_mode")]
public class QueryScriptDebugModeEvent : QueryEvent
{
    public override QueryType QueryType => QueryType.ScriptDebugMode;
    [EventPart("arg", true, (ushort)0)] public ushort Argument { get; set; }
    [EventPart("op", true, QueryOperation.AlwaysFalse)] public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
    [EventPart("imm", true, (byte)0)] public byte Immediate { get; private set; } // immediate value?
    QueryScriptDebugModeEvent() { }
    public QueryScriptDebugModeEvent(ushort argument, QueryOperation operation, byte immediate)
    {
        Argument = argument;
        Operation = operation;
        Immediate = immediate;
    }
    public static QueryScriptDebugModeEvent Serdes(QueryScriptDebugModeEvent e, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new QueryScriptDebugModeEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.Argument = s.UInt16(nameof(Argument), e.Argument);
        // field 8 is the next event id when the condition is false and is deserialised as part of the BranchEventNode that this event should belong to.

        s.Assert(zeroes == 0, "QueryScriptDebugModeEvent: Expected fields 3,4 to be 0");
        return e;
    }
}