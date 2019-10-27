using System.Diagnostics;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class DisableEventChainEvent : ModifyEvent
    {
        public DisableEventChainEvent(BinaryReader br, int id, EventType type, ModifyType subType) : base(id, type, subType)
        {
            Unk2 = br.ReadByte(); // 2
            Debug.Assert(Unk2 == 1 || Unk2 == 0 || Unk2 == 2); // Usually 1
            ChainNumber = br.ReadByte(); // 3
            Unk4 = br.ReadByte(); // 4
            Unk5 = br.ReadByte(); // 5
            Unk6 = br.ReadUInt16(); // 6
            Unk8 = br.ReadUInt16(); // 8
        }

        public byte Unk2 { get; }
        public byte ChainNumber { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Unk6 { get; }
        public ushort Unk8 { get; set; }
    }
}