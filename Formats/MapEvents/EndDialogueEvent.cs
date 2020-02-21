using System.IO;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class EndDialogueEvent : IMapEvent
    {
        public static EndDialogueEvent Translate(EndDialogueEvent node, ISerializer s)
        {
            node ??= new EndDialogueEvent();
            s.Dynamic(node, nameof(Unk1));
            s.Dynamic(node, nameof(Unk2));
            s.Dynamic(node, nameof(Unk3));
            s.Dynamic(node, nameof(Unk4));
            s.Dynamic(node, nameof(Unk5));
            s.Dynamic(node, nameof(Unk6));
            s.Dynamic(node, nameof(Unk8));
            return node;
        }
        public static EventNode Load(BinaryReader br, int id, MapEventType type)
        {
            return new EventNode(id, new EndDialogueEvent
            {
                Unk1 = br.ReadByte(),   // +1
                Unk2 = br.ReadByte(),   // +2
                Unk3 = br.ReadByte(),   // +3
                Unk4 = br.ReadByte(),   // +4
                Unk5 = br.ReadByte(),   // +5
                Unk6 = br.ReadUInt16(), // +6
                Unk8 = br.ReadUInt16()  // +8
            });
        }

        public byte Unk1 { get; private set; }
        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        public byte Unk4 { get; private set; }
        public byte Unk5 { get; private set; }
        public ushort Unk6 { get; private set; }
        public ushort Unk8 { get; private set; }
        public MapEventType EventType => MapEventType.EndDialogue;
    }
}
