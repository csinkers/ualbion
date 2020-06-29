using System;
using SerdesNet;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets.Map
{
    public class MapEventZone
    {
        public bool Global;
        public byte Unk1 { get; set; }
        public byte X;
        public byte Y;
        public TriggerType Trigger;
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
            ushort? nodeId = ConvertMaxToNull.Serdes(nameof(Node), zone.Node?.Id, s.UInt16);
            if (nodeId != null && zone.Node == null)
                zone.Node = new DummyEventNode(nodeId.Value);
            return zone;
        }

        public void Unswizzle(Func<ushort, (EventChain, IEventNode)> getEvent)
        {
            if (Node is DummyEventNode dummy)
                (Chain, Node) = getEvent(dummy.Id);
        }

        public override string ToString() => $"Zone ({X}, {Y}) T:{Trigger} Mode:{Unk1} C:{Chain?.Id} E:{Node?.Id}";
    }
}
