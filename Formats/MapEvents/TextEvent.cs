using UAlbion.Api;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    [Api.Event("text")]
    public class TextEvent : IMapEvent
    {
        public static TextEvent Serdes(TextEvent node, ISerializer s)
        {
            node ??= new TextEvent();
            s.Dynamic(node, nameof(TextType));
            s.Dynamic(node, nameof(Unk2));
            s.Dynamic(node, nameof(Unk3));
            s.Dynamic(node, nameof(PortraitId));
            s.Dynamic(node, nameof(TextId));
            s.Dynamic(node, nameof(Unk6));
            s.Dynamic(node, nameof(Unk8));
            return node;
        }
        /* public enum TextTypes
        {
            TextInWindow,
            TextInWindowWithPortrait,
            TextInWindowWithNpcPortrait,
            AddDialogOption,
            QuickInfo,
            DialogQuestion,
            DialogResponse,
            AddDefaultDialogOption,
            ListDefaultDialogOptions
        } */

        TextEvent() { }

        public TextEvent(byte textId)
        {
            TextId = textId;
        }

        [EventPart("text_id")] public byte TextId { get; private set; }
        public byte PortraitId { get; private set; }
        public byte TextType { get; private set; }
        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; private set; }
        public override string ToString() => $"text {TextId} {PortraitId} ({TextType} {Unk2} {Unk3} {Unk6} {Unk8})";
        public MapEventType EventType => MapEventType.Text;
    }
}
