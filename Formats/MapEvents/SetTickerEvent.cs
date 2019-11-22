using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class SetTickerEvent : ModifyEvent
    {
        public static EventNode Load(BinaryReader br, int id, MapEventType type, ModifyType subType)
        {
            return new EventNode(id, new SetTickerEvent
            {
                Operation = (QuantityChangeOperation) br.ReadByte(), // 2
                Amount = br.ReadByte(), // 3
                Unk4 = br.ReadByte(), // 4
                Unk5 = br.ReadByte(), // 5
                TickerId = br.ReadUInt16(), // 6
                Unk8 = br.ReadUInt16(), // 8
            });
        }

        public QuantityChangeOperation Operation { get; private set; }
        public byte Amount { get; set; }
        public ushort TickerId { get; private set; }

        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Unk8 { get; set; }
        public override string ToString() => $"set_ticker {TickerId} {Operation} {Amount} ({Unk4} {Unk5} {Unk8})";
    }
}
