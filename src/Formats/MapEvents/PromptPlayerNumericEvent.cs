using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("prompt_player_numeric")]
    public class PromptPlayerNumericEvent : QueryEvent
    {
        public override QueryType QueryType => QueryType.PromptPlayerNumeric;
        [EventPart("text_src")] public TextId TextSourceId { get; }
        [EventPart("op")] public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
        [EventPart("imm")] public byte Immediate { get; private set; } // immediate value?
        [EventPart("src")] public ushort Argument { get; set; }
        PromptPlayerNumericEvent(TextId textSourceId) => TextSourceId = textSourceId;
        public PromptPlayerNumericEvent(TextId textSourceId, QueryOperation operation, byte immediate, ushort argument)
        {
            TextSourceId = textSourceId;
            Operation = operation;
            Immediate = immediate;
            Argument = argument;
        }
        public static PromptPlayerNumericEvent Serdes(PromptPlayerNumericEvent e, TextId textSourceId, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new PromptPlayerNumericEvent(textSourceId);
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
            int zeroes = s.UInt8(null, 0);
            zeroes += s.UInt8(null, 0);
            e.Argument = s.UInt16(nameof(Argument), e.Argument);
            // field 8 is the next event id when the condition is false and is deserialised as part of the BranchEventNode that this event should belong to.

            s.Assert(zeroes == 0, "PromptPlayerNumericEvent: Expected fields 3,4 to be 0");
            return e;
        }
    }
}
