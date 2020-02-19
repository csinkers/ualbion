using System.Diagnostics;
using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class SetTickerEvent : ModifyEvent
    {
        public static EventNode Load(BinaryReader br, int id, MapEventType type, ModifyType subType)
        {
            var tickerEvent = new SetTickerEvent
            {
                Operation = (QuantityChangeOperation) br.ReadByte(), // 2
                Amount = br.ReadByte(), // 3
                Unk4 = br.ReadByte(), // 4
                Unk5 = br.ReadByte(), // 5
                TickerId = br.ReadUInt16(), // 6
                Unk8 = br.ReadUInt16(), // 8
            };
            Debug.Assert(tickerEvent.Unk4 == 0 || tickerEvent.Unk4 == 1);
            Debug.Assert(tickerEvent.Unk5 == 0);
            Debug.Assert(tickerEvent.Unk8 == 0);
            return new EventNode(id, tickerEvent);
        }

        public QuantityChangeOperation Operation { get; private set; }
        public byte Amount { get; set; }
        public ushort TickerId { get; private set; }

        public byte Unk4 { get; set; } // 0, 1
        byte Unk5 { get; set; }
        ushort Unk8 { get; set; }
        public override string ToString() => $"set_ticker {TickerId} {Operation} {Amount} ({Unk4})";
    }
}
