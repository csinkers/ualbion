using System;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    [Event("pause")] // USED IN SCRIPT
    public class PauseEvent : MapEvent
    {
        public static PauseEvent Serdes(PauseEvent e, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
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
}
