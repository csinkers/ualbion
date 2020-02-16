using System.Diagnostics;
using System.IO;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets
{
    public class MapEventZone
    {

        public bool Global;
        public byte Unk1 { get; set; }
        public ushort X;
        public ushort Y;
        public TriggerType Trigger;
        public ushort EventNumber;
        public IEventNode Event { get; set; }

        public static MapEventZone LoadGlobalZone(BinaryReader br)
        {
            var zone = new MapEventZone();
            zone.Global = true;
            zone.X = br.ReadUInt16(); // +0
            Debug.Assert(zone.X == 0);
            zone.Trigger = (TriggerType)br.ReadUInt16(); // +2
            zone.EventNumber = br.ReadUInt16(); // +4
            return zone;
        }

        public static MapEventZone LoadZone(BinaryReader br, ushort y)
        {
            var zone = new MapEventZone();
            zone.X = (ushort)(br.ReadByte() - 1); // +0
            Debug.Assert(zone.X != 0xffff);
            zone.Unk1 = br.ReadByte(); // +1
            zone.Y = y;
            zone.Trigger = (TriggerType)br.ReadUInt16(); // +2
            zone.EventNumber = br.ReadUInt16(); // +4
            return zone;
        }

        public override string ToString() => $"Zone ({X}, {Y}) T:{Trigger} Mode:{Unk1} E:{EventNumber}";
    }
}