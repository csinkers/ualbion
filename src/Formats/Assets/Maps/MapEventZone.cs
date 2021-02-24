using System;
using System.Globalization;
using System.Text.RegularExpressions;
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
        public AssetId ChainSource { get; set; }
        public ushort Chain { get; set; }
        public IEventNode Node { get; set; }

        public MapEventZone() => Key = new ZoneKey(this);
        public static MapEventZone Serdes(MapEventZone existing, ISerializer s, byte y)
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

        public void Unswizzle(MapId mapId, Func<ushort, IEventNode> getEvent, Func<ushort, ushort> getChain)
        {
            if (getEvent == null) throw new ArgumentNullException(nameof(getEvent));
            if (getChain == null) throw new ArgumentNullException(nameof(getChain));
            ChainSource = mapId;
            if (Node is DummyEventNode dummy)
            {
                Node = getEvent(dummy.Id);
                Chain = getChain(dummy.Id);
            }
            else Chain = 0xffff;
        }

        public override string ToString() => $"{(Global ? "GZ" : "Z")}({X}, {Y}) T({Trigger}) M({Unk1}) C({Chain}) E({Node?.Id})";
        static readonly Regex ZoneRegex = new Regex(
            @"
\s*(?<Type>Z|GZ)\((?<X>\d+),\s*(?<Y>\d+)\)\s*
T\((?<Trigger>[^)]+)\)\s*
M\((?<Mode>[^)]+)\)\s*
C\((?<Chain>[^)]+)\)\s*
E\((?<Event>[^)]+)\)\s*", RegexOptions.IgnorePatternWhitespace);
        public static MapEventZone Parse(string s)
        {
            var m = ZoneRegex.Match(s);
            if (!m.Success)
                throw new FormatException($"Could not parse \"{s}\" as a MapEventZone");

            return new MapEventZone
            {
                Global = m.Groups["Type"].Value == "GZ",
                X = byte.Parse(m.Groups["X"].Value, CultureInfo.InvariantCulture),
                Y = byte.Parse(m.Groups["Y"].Value, CultureInfo.InvariantCulture),
                Trigger = (TriggerTypes)Enum.Parse(typeof(TriggerTypes), m.Groups["Trigger"].Value),
                Unk1 = byte.Parse(m.Groups["Mode"].Value, CultureInfo.InvariantCulture),
                Chain = ushort.Parse(m.Groups["Chain"].Value, CultureInfo.InvariantCulture),
                Node = new DummyEventNode(ushort.Parse(m.Groups["Event"].Value, CultureInfo.InvariantCulture))
            };
        }
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
        public ushort Chain => _zone.Chain;
        public IEventNode Node => _zone.Node;
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Global.GetHashCode();
                hashCode = (hashCode * 397) ^ Unk1.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Trigger;
                hashCode = (hashCode * 397) ^ (Node != null ? Node.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Chain.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ZoneKey a, ZoneKey b) => a.Equals(b);
        public static bool operator !=(ZoneKey a, ZoneKey b) => !a.Equals(b);
        public override bool Equals(object obj) => obj is ZoneKey key && Equals(key);
        public bool Equals(ZoneKey other) => Global == other.Global && Unk1 == other.Unk1 && Trigger == other.Trigger && Equals(Node, other.Node);
    }
}
