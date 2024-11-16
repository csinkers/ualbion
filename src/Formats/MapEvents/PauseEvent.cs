using System;
using SerdesNet;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

[Event("pause")] // USED IN SCRIPT
public class PauseEvent : MapEvent
{
    public static PauseEvent Serdes(PauseEvent e, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new PauseEvent();
        e.Length = s.UInt8(nameof(Length), e.Length);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt16(null, 0);
        zeroes += s.UInt16(null, 0);
        s.Assert(zeroes == 0, "PauseEvent: Expected fields 3-8 to be 0");
        return e;
    }
    PauseEvent() { }
    public PauseEvent(byte length) => Length = length;

    [EventPart("length")] public byte Length { get; private set; }
    public override MapEventType EventType => MapEventType.Pause;
}