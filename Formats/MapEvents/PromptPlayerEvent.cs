using SerdesNet;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class PromptPlayerEvent : MapEvent, IQueryEvent, ITextEvent
    {
        public static PromptPlayerEvent Serdes(PromptPlayerEvent e, ISerializer s, AssetType textType, ushort textSourceId)
        {
            e ??= new PromptPlayerEvent(textType, textSourceId);
            s.Begin();
            e.Operation = s.EnumU8(nameof(Operation), e.Operation);
            e.Immediate = s.UInt8(nameof(Immediate), e.Immediate);
            s.UInt16("Padding", 0);
            e.TextId = s.UInt16(nameof(TextId), e.TextId);
            s.End();
            return e;
        }

        PromptPlayerEvent(AssetType textType, ushort textSourceId)
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

        public AssetType TextType { get; }
        public ushort TextSourceId { get; }
    }
}
