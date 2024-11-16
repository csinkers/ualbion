using System;
using SerdesNet;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

[Event("offset")]
public class OffsetEvent : MapEvent
{
    OffsetEvent() { }
    public OffsetEvent(sbyte x, sbyte y, byte unk3)
    {
        X = x;
        Y = y;
        Unk3 = unk3;
    }

    public static OffsetEvent Serdes(OffsetEvent e, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new OffsetEvent();
        e.X = s.Int8(nameof(X), e.X);
        e.Y = s.Int8(nameof(Y), e.Y);
        e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt16(null, 0);
        zeroes += s.UInt16(null, 0);
        s.Assert(e.Unk3 is 1 or 3, "OffsetEvent: Expected field 3 to be 1 or 3");
        s.Assert(zeroes == 0, "OffsetEvent: Expected fields 4-8 to be 0");
        return e;
    }

    [EventPart("x")] public sbyte X { get; private set; }
    [EventPart("y")] public sbyte Y { get; private set; }
    [EventPart("unk3")] public byte Unk3 { get; private set; }
    public override MapEventType EventType => MapEventType.Offset;
}