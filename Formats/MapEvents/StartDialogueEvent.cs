using System.Diagnostics;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class StartDialogueEvent : IMapEvent
    {
        public static StartDialogueEvent Translate(StartDialogueEvent e, ISerializer s)
        {
            e ??= new StartDialogueEvent();
            s.Dynamic(e, nameof(Unk1));
            s.Dynamic(e, nameof(Unk2));
            s.Dynamic(e, nameof(Unk3));
            s.Dynamic(e, nameof(Unk4));
            s.Dynamic(e, nameof(Unk5));
            s.Dynamic(e, nameof(Unk6));
            s.Dynamic(e, nameof(Unk8));
            Debug.Assert(e.Unk1 == 1);
            Debug.Assert(e.Unk2 == 0);
            Debug.Assert(e.Unk3 == 0);
            Debug.Assert(e.Unk4 == 0);
            Debug.Assert(e.Unk5 == 0);
            Debug.Assert(e.Unk8 == 0);
            return e;
        }

        byte Unk1 { get; set; }
        byte Unk2 { get; set; }
        byte Unk3 { get; set; }
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        public ushort Unk6 { get; private set; } // TODO: NpcId, EventId, string id?
        ushort Unk8 { get; set; }
        public override string ToString() => $"start_dialogue {Unk6}";
        public MapEventType EventType => MapEventType.StartDialogue;
    }
}
