using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("clone_automap", "Copy the automap discovery data from one map to another")]
public class CloneAutomapEvent : MapEvent
{
    public static CloneAutomapEvent Serdes(CloneAutomapEvent e, AssetMapping mapping, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new CloneAutomapEvent();
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        s.Assert(zeroes == 0, "CloneAutomap: Expected fields 1-5 to be 0");
        e.From = MapId.SerdesU16(nameof(From), e.From, mapping, s);
        e.To = MapId.SerdesU16(nameof(To), e.To, mapping, s);
        return e;
    }

    CloneAutomapEvent() { }
    public CloneAutomapEvent(MapId from, MapId to)
    {
        From = from;
        To = to;
    }

    [EventPart("from", "the map to copy from")] public MapId From { get; private set; }
    [EventPart("to", "the map to copy to")] public MapId To { get; private set; }
    public override MapEventType EventType => MapEventType.CloneAutomap;
}