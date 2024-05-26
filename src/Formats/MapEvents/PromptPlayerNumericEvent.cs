using System;
using SerdesNet;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

[Event("prompt_player_numeric")]
public class PromptPlayerNumericEvent : QueryEvent
{
    public override QueryType QueryType => QueryType.PromptPlayerNumeric;
    [EventPart("op")] public QueryOperation Operation { get; private set; }
    [EventPart("src")] public ushort Argument { get; set; } // The value to match
    PromptPlayerNumericEvent() { }
    public PromptPlayerNumericEvent(QueryOperation operation, ushort argument)
    {
        Operation = operation;
        Argument = argument;
    }
    public static PromptPlayerNumericEvent Serdes(PromptPlayerNumericEvent e, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new PromptPlayerNumericEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation); // 1

        int zeroes = s.UInt8(null, 0); // 2
        zeroes += s.UInt8(null, 0); // 3
        zeroes += s.UInt8(null, 0); // 4

        e.Argument = s.UInt16(nameof(Argument), e.Argument); // 6
        // field 8 is the next event id when the condition is false and is deserialised as part of the BranchEventNode that this event should belong to.

        s.Assert(zeroes == 0, "PromptPlayerNumericEvent: Expected fields 2,3,4 to be 0");
        return e;
    }
}