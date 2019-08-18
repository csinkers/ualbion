using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class OffsetEvent : MapEvent
    {
        public OffsetEvent(BinaryReader br, int id, EventType type) : base(id, type)
        {
            X = br.ReadSByte(); // +1
            Y = br.ReadSByte(); // +2
            Unk3 = br.ReadByte(); // +3
            Unk4 = br.ReadByte(); // +4
            Unk5 = br.ReadByte(); // +5
            Unk6 = br.ReadUInt16(); // +6
            Unk8 = br.ReadUInt16(); // +8
        }

        public sbyte X { get; }
        public sbyte Y { get; }
        public byte Unk3 { get; }
        public byte Unk4 { get; }
        public byte Unk5 { get; }
        public ushort Unk6 { get; }
        public ushort Unk8 { get; }
    }
}