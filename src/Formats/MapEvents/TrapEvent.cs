using System;
using SerdesNet;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

[Event("trap")]
public class TrapEvent : MapEvent
{
    TrapEvent() { }

    public TrapEvent(byte unk1, byte unk2, byte unk3, byte unk5, ushort unk6)
    {
        Unk1 = unk1;
        Unk2 = unk2;
        Unk3 = unk3;
        Unk5 = unk5;
        Unk6 = unk6;
    }

    public static TrapEvent Serdes(TrapEvent e, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new TrapEvent();
        e.Unk1 = s.UInt8(nameof(Unk1), e.Unk1);
        e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
        e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
        int zeroed = s.UInt8(null, 0);
        e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
        e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
        zeroed += s.UInt16(null, 0);
        s.Assert(zeroed == 0, "TrapEvent: Expected fields 4, 8 to be 0");
        return e;
    }

    [EventPart("unk1")] public byte Unk1 { get; private set; } // Observed values: 1,6,7,11,255
    [EventPart("unk2")] public byte Unk2 { get; private set; } // 2,3 (2 only seen once)
    [EventPart("unk3")] public byte Unk3 { get; private set; } // 0,1,2
    [EventPart("unk5")] public byte Unk5 { get; private set; } // [0..12]
    [EventPart("unk6")] public ushort Unk6 { get; private set; } // [0..10000], mostly 6, 0 or multiples of 5. Damage?

    public override MapEventType EventType => MapEventType.Trap;
}