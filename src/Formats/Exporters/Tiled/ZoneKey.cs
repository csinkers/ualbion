using System;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Maps;

namespace UAlbion.Formats.Exporters.Tiled;

public readonly struct ZoneKey : IEquatable<ZoneKey>
{
    public ZoneKey(MapEventZone zone)
    {
        if (zone == null) throw new ArgumentNullException(nameof(zone));
        Global = zone.Global;
        Unk1 = zone.Unk1;
        Trigger = zone.Trigger;
        Chain = zone.Chain;
        DummyNumber = 0;
        Node = zone.Node;
    }

    public ZoneKey(ushort chain, ushort dummyNumber, IEventNode node) // For dummy triggers
    {
        Global = true;
        Unk1 = 0;
        Trigger = 0;
        Chain = chain;
        DummyNumber = dummyNumber;
        Node = node;
    }

    public ZoneKey(bool global, TriggerTypes type, ushort chain) // For tests
    {
        Global = global;
        Trigger = type;
        Chain = chain;
        Unk1 = 0;
        DummyNumber = 0;
        Node = null;
    }

    public bool Global { get; }
    public byte Unk1 { get; }
    public TriggerTypes Trigger { get; }
    public ushort Chain { get; }
    public ushort DummyNumber { get; } // 0 for actual chains, higher numbers for unchained events
    public IEventNode Node { get; }
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
    public override string ToString() => $"{(Global ? 'G' : 'L')}{Chain}{(DummyNumber == 0 ? "" : $".{DummyNumber}")}:{Trigger} ({Unk1})";
}