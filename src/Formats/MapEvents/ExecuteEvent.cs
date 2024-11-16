using System;
using SerdesNet;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

[Event("execute")]
public class ExecuteEvent : MapEvent
{
    ExecuteEvent() { }
    public ExecuteEvent(byte unk1, ushort unk8)
    {
        Unk1 = unk1;
        Unk8 = unk8;
    }

    public static ExecuteEvent Serdes(ExecuteEvent e, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new ExecuteEvent();
        e.Unk1 = s.UInt8(nameof(Unk1), e.Unk1);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt16(null, 0);
        e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
        s.Assert(zeroes == 0, "ExecuteEvent: Expected fields 2-6 to be 0");
        return e;
    }

    [EventPart("unk1")]public byte Unk1 { get; private set; }
    [EventPart("unk8")]public ushort Unk8 { get; private set; }
    public override MapEventType EventType => MapEventType.Execute;
}