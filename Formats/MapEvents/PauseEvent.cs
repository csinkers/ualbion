using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class PauseEvent : MapEvent
    {
        public PauseEvent(BinaryReader br, int id, EventType type) : base(id, type)
        {
            Unk1 = br.ReadByte(); // +1
            Unk2 = br.ReadByte(); // +2
            Unk3 = br.ReadByte(); // +3
            Unk4 = br.ReadByte(); // +4
            Unk5 = br.ReadByte(); // +5
            Unk6 = br.ReadUInt16(); // +6
            Unk8 = br.ReadUInt16(); // +8
        }

        public byte Unk1 { get; }
        public byte Unk2 { get; }
        public byte Unk3 { get; }
        public byte Unk4 { get; }
        public byte Unk5 { get; }
        public ushort Unk6 { get; }
        public ushort Unk8 { get; }
    }
}