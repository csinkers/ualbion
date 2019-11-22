using System.IO;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public class DoScriptEvent : IEvent
    {
        public static EventNode Load(BinaryReader br, int id)
        {
            return new EventNode(id, new DoScriptEvent
            {
                Unk1 = br.ReadByte(), // +1
                Unk2 = br.ReadByte(), // +2
                Unk3 = br.ReadByte(), // +3
                Unk4 = br.ReadByte(), // +4
                Unk5 = br.ReadByte(), // +5
                ScriptId = br.ReadUInt16(), // +6
                Unk8 = br.ReadUInt16(), // +8
            });
        }

        public ushort ScriptId { get; private set; }

        public byte Unk1 { get; private set; }
        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        public byte Unk4 { get; private set; }
        public byte Unk5 { get; private set; }
        public ushort Unk8 { get; private set; }
        public override string ToString() => $"do_script {ScriptId} ({Unk1} {Unk2} {Unk3} {Unk4} {Unk5} {Unk8})";
    }
}
