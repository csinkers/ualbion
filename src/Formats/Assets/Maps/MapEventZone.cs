using System;
using SerdesNet;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets.Maps
{
    public class MapEventZone
    {
        public bool Global { get; set; }
        public byte Unk1 { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
        public TriggerTypes Trigger { get; set; }
        public EventChain Chain { get; set; }
        public IEventNode Node { get; set; }

        public static MapEventZone Serdes(MapEventZone existing, ISerializer s, in byte y)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            bool global = y == 0xff;
            var zone = existing ?? new MapEventZone
            {
                Global = global,
                Y = global ? (byte)0 : y
            };

            zone.X = s.Transform<byte, byte>(nameof(X), zone.X, S.UInt8, StoreIncrementedConverter.Instance);
            // ApiUtil.Assert(global && zone.X == 0xff || !global && zone.X != 0xff);
            zone.Unk1 = s.UInt8(nameof(Unk1), zone.Unk1);
            zone.Trigger = s.EnumU16(nameof(Trigger), zone.Trigger);
            ushort? nodeId = s.Transform<ushort, ushort?>(nameof(Node), zone.Node?.Id, S.UInt16, MaxToNullConverter.Instance);
            if (nodeId != null && zone.Node == null)
                zone.Node = new DummyEventNode(nodeId.Value);

            return zone;
        }

        public void Unswizzle(Func<ushort, (EventChain, IEventNode)> getEvent)
        {
            if (getEvent == null) throw new ArgumentNullException(nameof(getEvent));
            if (Node is DummyEventNode dummy)
                (Chain, Node) = getEvent(dummy.Id);
        }

        public override string ToString() => $"Zone ({X}, {Y}) T:{Trigger} Mode:{Unk1} C:{Chain?.Id} E:{Node?.Id}";
    }
}
