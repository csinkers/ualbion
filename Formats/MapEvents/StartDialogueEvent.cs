using System.Diagnostics;
using UAlbion.Api;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class StartDialogueEvent : Event, IMapEvent
    {
        public static StartDialogueEvent Serdes(StartDialogueEvent e, ISerializer s)
        {
            e ??= new StartDialogueEvent();
            e.Unk1 = s.UInt8(nameof(Unk1), e.Unk1);
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
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
