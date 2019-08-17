using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class TextEvent : MapEvent
    {
        public enum TextType
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
        }

        public TextEvent(BinaryReader br, int id, EventType type) : base(id, type)
        {
            Unk1 = br.ReadByte(); // +1
            Unk2 = br.ReadByte(); // +2
            Unk3 = br.ReadByte(); // +3
            PortraitId = br.ReadByte(); // +4
            TextId = br.ReadByte(); // +5
            Unk6 = br.ReadUInt16(); // +6
            Unk8 = br.ReadUInt16(); // +8
        }

        public byte PortraitId { get; }
        public byte TextId { get; }
        public byte Unk1 { get; }
        public byte Unk2 { get; }
        public byte Unk3 { get; }
        public ushort Unk6 { get; }
        public ushort Unk8 { get; }
    }
}