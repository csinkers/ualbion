using UAlbion.Formats.MapEvents;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets
{
    public class MapEventZone
    {
        public bool Global;
        public byte Unk1 { get; set; }
        public byte X;
        public byte Y;
        public TriggerType Trigger;
        public ushort? EventNumber { get; set; }
        public EventChain Chain { get; set; }
        public IEventNode Node { get; set; }

        public static MapEventZone Serdes(MapEventZone existing, ISerializer s, byte y)
        {
            bool global = y == 0xff;
            var zone = existing ?? new MapEventZone
            {
                Global = global,
                Y = global ? (byte)0 : y
            };

            zone.X = s.Transform<byte, byte>(nameof(X), zone.X, s.UInt8, StoreIncremented.Instance);
            // ApiUtil.Assert(global && zone.X == 0xff || !global && zone.X != 0xff);
            zone.Unk1 = s.UInt8(nameof(Unk1), zone.Unk1);
            zone.Trigger = s.EnumU16(nameof(Trigger), zone.Trigger);
            zone.EventNumber = ConvertMaxToNull.Serdes(nameof(EventNumber), zone.EventNumber, s.UInt16);
            return zone;
        }

        public override string ToString() => $"Zone ({X}, {Y}) T:{Trigger} Mode:{Unk1} C:{Chain.Id} E:{EventNumber}";
    }
}
