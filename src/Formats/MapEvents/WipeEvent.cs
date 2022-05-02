using System;
using SerdesNet;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

[Event("wipe")]
public class WipeEvent : MapEvent
{
    WipeEvent() { }
    public WipeEvent(byte value) => Value = value;

    public static WipeEvent Serdes(WipeEvent e, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new WipeEvent();
        e.Value = s.UInt8(nameof(Value), e.Value);
        int zeroed = s.UInt8(null, 0);
        zeroed += s.UInt8(null, 0);
        zeroed += s.UInt8(null, 0);
        zeroed += s.UInt8(null, 0);
        zeroed += s.UInt16(null, 0);
        zeroed += s.UInt16(null, 0);
        s.Assert(zeroed == 0, "WipeEvent: Expected fields 3-8 to be 0");
        return e;
    }

    [EventPart("value")] public byte Value { get; private set; }
    public override MapEventType EventType => MapEventType.Wipe;
}