using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    [Event("end_dialogue", null, new [] { "ed" })]
    public class EndDialogueEvent : MapEvent
    {
        public static EndDialogueEvent Serdes(EndDialogueEvent e, ISerializer s)
        {
            e ??= new EndDialogueEvent();
            s.Begin();
            s.UInt8("Pad1", 0);
            s.UInt8("Pad2", 0);
            s.UInt8("Pad3", 0);
            s.UInt8("Pad4", 0);
            s.UInt8("Pad5", 0);
            s.UInt16("Pad6", 0);
            s.UInt16("Pad8", 0);
            s.End();
            return e;
        }

        public override MapEventType EventType => MapEventType.EndDialogue;
    }
}
