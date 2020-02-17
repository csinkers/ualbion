using System.IO;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    [Event("text")]
    public class TextEvent : IEvent
    {
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

        public static EventNode Load(BinaryReader br, int id, MapEventType type) =>
            new EventNode(id, new TextEvent
            {
                TextType = br.ReadByte(), // +1
                Unk2 = br.ReadByte(), // +2
                Unk3 = br.ReadByte(), // +3
                PortraitId = br.ReadByte(), // +4
                TextId = br.ReadByte(), // +5
                Unk6 = br.ReadUInt16(), // +6
                Unk8 = br.ReadUInt16(), // +8
            });
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
    }
}
