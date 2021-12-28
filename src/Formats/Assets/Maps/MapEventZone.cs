using System;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Assets.Maps;

public class MapEventZone 
{
    public bool Global { get; set; }
    public byte Unk1 { get; set; }
    public byte X { get; set; }
    public byte Y { get; set; }
    public TriggerTypes Trigger { get; set; }
    [JsonIgnore] public AssetId ChainSource { get; set; }
    [JsonIgnore] public ushort Chain { get; set; }
    [JsonIgnore] public IEventNode Node { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)] public ushort EventIndex
    {
        get => Node?.Id ?? 0xffff;
        set => Node = value == 0xffff ? null : new DummyEventNode(value);
    }

    public static MapEventZone Serdes(MapEventZone existing, ISerializer s, byte y)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));

        s.Begin("Zone");
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

        s.End();
        return zone;
    }

    public void Unswizzle(MapId mapId, Func<ushort, IEventNode> getEvent, Func<ushort, ushort> getChain)
    {
        if (getEvent == null) throw new ArgumentNullException(nameof(getEvent));
        if (getChain == null) throw new ArgumentNullException(nameof(getChain));
        ChainSource = mapId;
        if (Node is DummyEventNode dummy)
            Node = dummy.Id == EventNode.UnusedEventId ? null : getEvent(dummy.Id);

        if (Node != null)
            Chain = getChain(Node.Id);
    }

    public override string ToString() => $"{(Global ? "GZ" : "Z")}({X}, {Y}) T({Trigger}) M({Unk1}) C({Chain}) E({Node?.Id})";
    static readonly Regex ZoneRegex = new(
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