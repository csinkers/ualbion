using System;
using SerdesNet;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

[Event("signal")]
public class SignalEvent : MapEvent
{
    SignalEvent() { }
    public SignalEvent(byte signalId) => SignalId = signalId;

    public static SignalEvent Serdes(SignalEvent e, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new SignalEvent();
        e.SignalId = s.UInt8(nameof(SignalId), e.SignalId);
        int zeroed = s.UInt8(null, 0);
        zeroed += s.UInt8(null, 0);
        zeroed += s.UInt8(null, 0);
        zeroed += s.UInt8(null, 0);
        zeroed += s.UInt16(null, 0);
        zeroed += s.UInt16(null, 0);
        s.Assert(zeroed == 0, "SignalEvent: Expected fields 3-8 to be 0");
        return e;
    }

    [EventPart("id")] public byte SignalId { get; private set; }
    public override MapEventType EventType => MapEventType.Signal;
}