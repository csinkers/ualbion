using System;
using SerdesNet;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

[Event("encounter")]
public class EncounterEvent : MapEvent
{
    EncounterEvent() { }
    public EncounterEvent(ushort unk6, ushort unk8)
    {
        Unk6 = unk6;
        Unk8 = unk8;
    }

    public static EncounterEvent Serdes(EncounterEvent e, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new EncounterEvent();
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
        e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
        s.Assert(zeroes ==0, "EncounterEvent: Expected fields 1-5 to be 0");
        return e;
    }

    [EventPart("unk6")] public ushort Unk6 { get; private set; }
    [EventPart("unk8")] public ushort Unk8 { get; private set; }
    public override MapEventType EventType => MapEventType.Encounter;
}