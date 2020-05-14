using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class PromptPlayerEvent : AsyncMapEvent, IQueryEvent, ITextEvent
    {
        public static PromptPlayerEvent Serdes(PromptPlayerEvent e, ISerializer s, AssetType textType, int textSourceId)
        {
            e ??= new PromptPlayerEvent(textType, textSourceId);
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
            s.UInt16("Padding", 0);
            e.TextId = s.UInt16(nameof(TextId), e.TextId);
            e.FalseEventId = ConvertMaxToNull.Serdes(nameof(FalseEventId), e.FalseEventId, s.UInt16);
            return e;
        }

        PromptPlayerEvent(AssetType textType, int textSourceId)
        {
            TextType = textType;
            TextSourceId = textSourceId;
        }

        public QueryType QueryType => QueryType.PromptPlayer;
        public QueryOperation Operation { get; private set; } // method to use for check? 0,1,2,3,4,5
        public byte Immediate { get; private set; } // immediate value?
        public ushort TextId { get; private set; }

        public override string ToString() => $"query {QueryType} {TextId} ({Operation} {Immediate})";
        public override MapEventType EventType => MapEventType.Query;
        public ushort? FalseEventId { get; set; }
        protected override AsyncEvent Clone() =>
            new PromptPlayerEvent(TextType, TextSourceId)
            {
                Operation = Operation,
                Immediate = Immediate,
                TextId = TextId,
                FalseEventId = FalseEventId,
            };

        public AssetType TextType { get; }
        public int TextSourceId { get; }
    }
}