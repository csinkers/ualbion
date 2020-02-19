using System.Diagnostics;
using System.IO;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public class WipeEvent : IEvent
    {
        public static EventNode Load(BinaryReader br, int id, MapEventType type)
        {
            var wipeEvent = new WipeEvent
            {
                Value = br.ReadByte(), // +1
                Unk2 = br.ReadByte(), // +2
                Unk3 = br.ReadByte(), // +3
                Unk4 = br.ReadByte(), // +4
                Unk5 = br.ReadByte(), // +5
                Unk6 = br.ReadUInt16(), // +6
                Unk8 = br.ReadUInt16(), // +8
            };

            Debug.Assert(wipeEvent.Unk2 == 0);
            Debug.Assert(wipeEvent.Unk3 == 0);
            Debug.Assert(wipeEvent.Unk4 == 0);
            Debug.Assert(wipeEvent.Unk5 == 0);
            Debug.Assert(wipeEvent.Unk6 == 0);
            Debug.Assert(wipeEvent.Unk8 == 0);
            return new EventNode(id, wipeEvent);
        }

        public byte Value { get; private set; }
        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        public byte Unk4 { get; private set; }
        public byte Unk5 { get; private set; }
        public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; private set; }
        public override string ToString() => $"wipe {Value}";
    }
}
