using System.Diagnostics;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class TeleportEvent : MapEvent
    {
        public TeleportEvent(BinaryReader br, int id, EventType type) : base(id, type)
        {
            X = br.ReadByte(); // +1
            Y = br.ReadByte(); // +2
            Direction = br.ReadByte(); // +3
            Always255 = br.ReadByte(); // +4
            Debug.Assert(Always255 == 255);
            Unk5 = br.ReadByte(); // +5
            MapId = br.ReadUInt16(); // +6
            Unk8 = br.ReadUInt16(); // +8
        }

        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Direction { get; set; }
        public byte Always255 { get; set; }
        public ushort MapId { get; set; } // 0 = stay on current map

        public byte Unk5 { get; set; }
        public ushort Unk8 { get; set; }
    }
}