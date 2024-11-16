using System;
using SerdesNet;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

[Event("end_dialogue", null, "ed")]
public class EndDialogueEvent : MapEvent
{
    public static EndDialogueEvent Serdes(EndDialogueEvent e, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new EndDialogueEvent();
        s.UInt8("Pad1", 0);
        s.UInt8("Pad2", 0);
        s.UInt8("Pad3", 0);
        s.UInt8("Pad4", 0);
        s.UInt8("Pad5", 0);
        s.UInt16("Pad6", 0);
        s.UInt16("Pad8", 0);
        return e;
    }

    public override MapEventType EventType => MapEventType.EndDialogue;
}