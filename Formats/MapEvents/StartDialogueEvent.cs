using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    [Event("start_dialogue")]
    public class StartDialogueEvent : AsyncEvent
    {
        StartDialogueEvent() { }
        public StartDialogueEvent(EventSetId eventSetId) => EventSet = eventSetId;

        [EventPart("event_set")]
        public EventSetId EventSet { get; private set; }

        public static StartDialogueEvent Serdes(StartDialogueEvent e, ISerializer s)
        {
            e ??= new StartDialogueEvent();
            s.UInt8("Pad1", 1);
            s.UInt8("Pad2", 0);
            s.UInt8("Pad3", 0);
            s.UInt8("Pad4", 0);
            s.UInt8("Pad5", 0);
            e.EventSet = s.EnumU16(nameof(EventSet), e.EventSet);
            s.UInt16("Pad8", 0);
            return e;
        }

        public override MapEventType EventType => MapEventType.StartDialogue;
        protected override AsyncEvent Clone() => new StartDialogueEvent(EventSet);
    }
}
