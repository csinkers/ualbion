using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class ChangeTimeEvent : ModifyEvent
    {
        public static EventNode Load(BinaryReader br, int id, MapEventType type, ModifyType subType)
        {
            return new EventNode(id, new ChangeTimeEvent
            {
                Operation = (QuantityChangeOperation) br.ReadByte(), // 2
                Unk3 = br.ReadByte(), // 3
                Unk4 = br.ReadByte(), // 4
                Unk5 = br.ReadByte(), // 5
                Amount = br.ReadUInt16(), // 6
                Unk8 = br.ReadUInt16(), // 8
            });
        }

        public QuantityChangeOperation Operation { get; private set; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Amount { get; private set; }
        public ushort Unk8 { get; set; }
        public override string ToString() => $"change_time {Operation} {Amount} ({Unk3} {Unk4} {Unk5} {Unk8})";
    }
}
