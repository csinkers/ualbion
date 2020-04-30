using SerdesNet;

namespace UAlbion.Formats.MapEvents
{
    public class PromptPlayerEvent : AsyncMapEvent, IQueryEvent
    {
        public static PromptPlayerEvent Serdes(PromptPlayerEvent e, ISerializer s)
        {
            e ??= new PromptPlayerEvent();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
            s.UInt16("Padding", 0);
            e.TextId = s.UInt16(nameof(TextId), e.TextId);
            e.FalseEventId = ConvertMaxToNull.Serdes(nameof(FalseEventId), e.FalseEventId, s.UInt16);
            return e;
        }

        public QueryType QueryType => QueryType.PromptPlayer;
        public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
        public byte Immediate { get; private set; } // immediate value?
        public ushort TextId { get; private set; }

        public override string ToString() => $"query {QueryType} {TextId} ({Operation} {Immediate})";
        public override MapEventType EventType => MapEventType.Query;
        public ushort? FalseEventId { get; set; }
        protected override AsyncEvent Clone() =>
            new PromptPlayerEvent
            {
                Operation = Operation,
                Immediate = Immediate,
                TextId = TextId,
                FalseEventId = FalseEventId,
                Context = Context
            };
    }
}