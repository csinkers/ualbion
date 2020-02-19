using System.Diagnostics;
using System.IO;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public class OffsetEvent : IEvent
    {
        public static EventNode Load(BinaryReader br, int id, MapEventType type)
        {
            var e = new OffsetEvent
            {
                X = br.ReadSByte(), // +1
                Y = br.ReadSByte(), // +2
                Unk3 = br.ReadByte(), // +3
                Unk4 = br.ReadByte(), // +4
                Unk5 = br.ReadByte(), // +5
                Unk6 = br.ReadUInt16(), // +6
                Unk8 = br.ReadUInt16(), // +8
            };
            Debug.Assert(e.Unk3 == 1 || e.Unk3 == 3);
            Debug.Assert(e.Unk4 == 0);
            Debug.Assert(e.Unk5 == 0);
            Debug.Assert(e.Unk6 == 0);
            Debug.Assert(e.Unk8 == 0);
            return new EventNode(id, e);
        }

        public sbyte X { get; private set; }
        public sbyte Y { get; private set; }
        public byte Unk3 { get; private set; }
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        ushort Unk6 { get; set; }
        ushort Unk8 { get; set; }
        public override string ToString() => $"offset <{X}, {Y}> ({Unk3} {Unk4} {Unk5} {Unk6} {Unk8})";
    }
}
