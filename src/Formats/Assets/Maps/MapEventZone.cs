using System;
using Newtonsoft.Json;
using SerdesNet;
using UAlbion.Config;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Formats.Assets.Maps
{
    [JsonConverter(typeof(ToStringJsonConverter))]
    public class MapEventZone 
    {
        public ZoneKey Key { get; }
        public bool Global { get; set; }
        public byte Unk1 { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
        public TriggerTypes Trigger { get; set; }
        public EventChain Chain { get; set; }
        public IEventNode Node { get; set; }

        public MapEventZone() => Key = new ZoneKey(this);
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

        public override string ToString() => $"Z({X}, {Y}) T:{Trigger} Mode:{Unk1} C:{Chain?.Id} E:{Node?.Id}";
    }

    public readonly struct ZoneKey : IEquatable<ZoneKey>
    {
        readonly MapEventZone _zone;

        public ZoneKey(MapEventZone zone)
        {
            _zone = zone ?? throw new ArgumentNullException(nameof(zone));
        }

        public bool Global => _zone.Global;
        public byte Unk1 => _zone.Unk1;
        public TriggerTypes Trigger => _zone.Trigger;
        public EventChain Chain => _zone.Chain;
        public IEventNode Node => _zone.Node;
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Global.GetHashCode();
                hashCode = (hashCode * 397) ^ Unk1.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Trigger;
                hashCode = (hashCode * 397) ^ (Node != null ? Node.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Chain != null ? Chain.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ZoneKey a, ZoneKey b) => a.Equals(b);
        public static bool operator !=(ZoneKey a, ZoneKey b) => !a.Equals(b);
        public override bool Equals(object obj) => obj is ZoneKey key && Equals(key);
        public bool Equals(ZoneKey other) => Global == other.Global && Unk1 == other.Unk1 && Trigger == other.Trigger && Equals(Node, other.Node);
    }
}
