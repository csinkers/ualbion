using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

[Event("prompt_player")]
public class PromptPlayerEvent : QueryEvent
{
    public override QueryType QueryType => QueryType.PromptPlayer;
    [EventPart("text_src")] public TextId TextSourceId { get; }
    [EventPart("arg")] public ushort Argument { get; set; }
    [EventPart("op", true, QueryOperation.IsTrue)] public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
    [EventPart("imm", true, (byte)0)] public byte Immediate { get; private set; } // immediate value?
    PromptPlayerEvent(TextId textSourceId) => TextSourceId = textSourceId;
    public PromptPlayerEvent(TextId textSourceId, ushort argument, QueryOperation operation, byte immediate)
    {
        TextSourceId = textSourceId;
        Argument = argument;
        Operation = operation;
        Immediate = immediate;
    }

    public static PromptPlayerEvent Serdes(PromptPlayerEvent e, TextId textSourceId, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new PromptPlayerEvent(textSourceId);
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.Argument = s.UInt16(nameof(Argument), e.Argument);
        // field 8 is the next event id when the condition is false and is deserialised as part of the BranchEventNode that this event should belong to.

        s.Assert(zeroes == 0, "PromptPlayerEvent: Expected fields 3,4 to be 0");
        return e;
    }
}