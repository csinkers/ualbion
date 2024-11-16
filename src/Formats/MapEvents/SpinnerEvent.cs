using System;
using SerdesNet;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

[Event("spinner")]
public class SpinnerEvent : MapEvent
{
    SpinnerEvent() { }
    public SpinnerEvent(byte unk1) => Unk1 = unk1;

    public static SpinnerEvent Serdes(SpinnerEvent e, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new SpinnerEvent();
        e.Unk1 = s.UInt8(nameof(Unk1), e.Unk1);
        int zeroed = s.UInt8(null, 0);
        zeroed += s.UInt8(null, 0);
        zeroed += s.UInt8(null, 0);
        zeroed += s.UInt8(null, 0);
        zeroed += s.UInt16(null, 0);
        zeroed += s.UInt16(null, 0);
        s.Assert(zeroed == 0, "SpinnerEvent: Expected fields 3-8 to be 0");
        return e;
    }

    [EventPart("unk1")] public byte Unk1 { get; private set; }
    public override MapEventType EventType => MapEventType.Spinner;
}