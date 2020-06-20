using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public class PromptPlayerNumericEvent : AsyncMapEvent, IQueryEvent
    {
        public static PromptPlayerNumericEvent Serdes(PromptPlayerNumericEvent e, ISerializer s)
        {
            e ??= new PromptPlayerNumericEvent();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
            s.UInt16("Padding", 0);
            e.Argument = s.UInt16(nameof(Argument), e.Argument);
            e.FalseEventId = ConvertMaxToNull.Serdes(nameof(FalseEventId), e.FalseEventId, s.UInt16);
            return e;
        }

        public QueryType QueryType => QueryType.PromptPlayerNumeric;
        public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
        public byte Immediate { get; private set; } // immediate value?
        public ushort Argument { get; private set; }

        public override string ToString() => $"query {QueryType} {Argument} ({Operation} {Immediate})";
        public override MapEventType EventType => MapEventType.Query;
        public ushort? FalseEventId { get; set; }
    }
}