using System.Diagnostics;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class DisableEventChainEvent : ModifyEvent
    {
        public static EventNode Load(BinaryReader br, int id, MapEventType type, ModifyType subType)
        {
            var e = new DisableEventChainEvent
            {
                Unk2 = br.ReadByte(), // 2
                ChainNumber = br.ReadByte(), // 3
                Unk4 = br.ReadByte(), // 4
                Unk5 = br.ReadByte(), // 5
                Unk6 = br.ReadUInt16(), // 6
                Unk8 = br.ReadUInt16(), // 8
            };
            Debug.Assert(e.Unk2 == 1 || e.Unk2 == 0 || e.Unk2 == 2); // Usually 1
            return new EventNode(id, e);
        }

        public byte Unk2 { get; private set; }
        public byte ChainNumber { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; set; }
        public override string ToString() => $"disable_event_chain {ChainNumber} ({Unk2} {Unk4} {Unk5} {Unk6} {Unk8})";
    }
}
