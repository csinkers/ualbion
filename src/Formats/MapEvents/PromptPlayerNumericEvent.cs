using SerdesNet;

namespace UAlbion.Formats.MapEvents
{
    public class PromptPlayerNumericEvent : MapEvent, IQueryEvent
    {
        public static PromptPlayerNumericEvent Serdes(PromptPlayerNumericEvent e, ISerializer s)
        {
            e ??= new PromptPlayerNumericEvent();
            s.Begin();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
            s.UInt16("Padding", 0);
            e.Argument = s.UInt16(nameof(Argument), e.Argument);
            s.End();
            return e;
        }

        public QueryType QueryType => QueryType.PromptPlayerNumeric;
        public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
        public byte Immediate { get; private set; } // immediate value?
        public ushort Argument { get; private set; }

        public override string ToString() => $"query {QueryType} {Argument} ({Operation} {Immediate})";
        public override MapEventType EventType => MapEventType.Query;
    }
}
