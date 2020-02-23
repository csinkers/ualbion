using UAlbion.Api;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    [Api.Event("text")]
    public class TextEvent : IMapEvent
    {
        public static TextEvent Serdes(TextEvent e, ISerializer s)
        {
            e ??= new TextEvent();
            e.TextType = s.UInt8(nameof(TextType), e.TextType);
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.PortraitId = s.UInt8(nameof(PortraitId), e.PortraitId);
            e.TextId = s.UInt8(nameof(TextId), e.TextId);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            return e;
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
