using System.Diagnostics;
using System.IO;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public class StartDialogueEvent : IEvent
    {
        public static EventNode Load(BinaryReader br, int id, MapEventType type)
        {
            var dialogueEvent = new StartDialogueEvent
            {
                Unk1 = br.ReadByte(), // +1
                Unk2 = br.ReadByte(), // +2
                Unk3 = br.ReadByte(), // +3
                Unk4 = br.ReadByte(), // +4
                Unk5 = br.ReadByte(), // +5
                Unk6 = br.ReadUInt16(), // +6
                Unk8 = br.ReadUInt16(), // +8
            };
            Debug.Assert(dialogueEvent.Unk1 == 1);
            Debug.Assert(dialogueEvent.Unk2 == 0);
            Debug.Assert(dialogueEvent.Unk3 == 0);
            Debug.Assert(dialogueEvent.Unk4 == 0);
            Debug.Assert(dialogueEvent.Unk5 == 0);
            Debug.Assert(dialogueEvent.Unk8 == 0);
            return new EventNode(id, dialogueEvent);
        }

        byte Unk1 { get; set; }
        byte Unk2 { get; set; }
        byte Unk3 { get; set; }
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        public ushort Unk6 { get; private set; } // TODO: NpcId, EventId, string id?
        ushort Unk8 { get; set; }
        public override string ToString() => $"start_dialogue {Unk6}";
    }
}
