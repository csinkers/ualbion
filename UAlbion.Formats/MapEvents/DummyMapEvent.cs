using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class DummyMapEvent : MapEvent
    {
        public DummyMapEvent(EventType type, int id, BinaryReader br)
        {
            Type = type;
            Unk1 = br.ReadByte(); // +1
            Unk2 = br.ReadByte(); // +2
            Unk3 = br.ReadByte(); // +3
            Unk4 = br.ReadByte(); // +4
            Unk5 = br.ReadByte(); // +5
            Unk6 = br.ReadUInt16(); // +6
            Unk8 = br.ReadUInt16(); // +8
            NextEventId = br.ReadUInt16(); // +A
            if (NextEventId == 0xffff) NextEventId = null;
        }

        public override EventType Type { get; }
        public byte Unk1 { get; }
        public byte Unk2 { get; }
        public byte Unk3 { get; }
        public byte Unk4 { get; }
        public byte Unk5 { get; }
        public ushort Unk6 { get; }
        public ushort Unk8 { get; }
        public override string ToString() => $"Event{Id}->{NextEventId}: {Type} {Unk1} {Unk2} {Unk3} {Unk4} {Unk5} {Unk6} {Unk8}";
    }
}