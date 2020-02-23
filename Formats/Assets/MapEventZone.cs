using System.Diagnostics;
using System.IO;
using UAlbion.Formats.MapEvents;
using UAlbion.Formats.Parsers;

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
        public IEventNode EventNode { get; set; }

        public static MapEventZone Serdes(MapEventZone existing, ISerializer s, ushort y)
        {
            bool global = y == 0xffff;
            var zone = existing ?? new MapEventZone
            {
                Global = global,
                Y = global ? (byte)0 : y
            };

            zone.X = s.Transform<ushort, ushort>(nameof(X), zone.X, s.UInt16, StoreIncremented.Instance);
            s.Dynamic(zone, nameof(Unk1));
            s.Dynamic(zone, nameof(Trigger));
            s.Dynamic(zone, nameof(EventNumber));
            return zone;
        }

        public static MapEventZone LoadZone(BinaryReader br, ushort y)
        {
            bool global = y == 0xffff;

            var zone = new MapEventZone();
            zone.Global = global;
            zone.X = (ushort)(br.ReadByte() - 1); // +0
            Debug.Assert(global && zone.X == 0xffff || !global && zone.X != 0xffff);
            zone.Unk1 = br.ReadByte(); // +1
            zone.Y = global ? (byte)0 : y;
            zone.Trigger = (TriggerType)br.ReadUInt16(); // +2
            zone.EventNumber = br.ReadUInt16(); // +4
            return zone;
        }

        public override string ToString() => $"Zone ({X}, {Y}) T:{Trigger} Mode:{Unk1} E:{EventNumber}";
    }
}