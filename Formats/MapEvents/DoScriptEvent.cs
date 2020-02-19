using System.Diagnostics;
using System.IO;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public class DoScriptEvent : IEvent
    {
        public static EventNode Load(BinaryReader br, int id)
        {
            var e = new DoScriptEvent
            {
                Unk1 = br.ReadByte(), // +1
                Unk2 = br.ReadByte(), // +2
                Unk3 = br.ReadByte(), // +3
                Unk4 = br.ReadByte(), // +4
                Unk5 = br.ReadByte(), // +5
                ScriptId = br.ReadUInt16(), // +6
                Unk8 = br.ReadUInt16(), // +8
            };
            Debug.Assert(e.Unk1 == 0);
            Debug.Assert(e.Unk2 == 0);
            Debug.Assert(e.Unk3 == 0);
            Debug.Assert(e.Unk4 == 0);
            Debug.Assert(e.Unk5 == 0);
            Debug.Assert(e.Unk8 == 0);
            return new EventNode(id, e);
        }

        public ushort ScriptId { get; private set; }

        byte Unk1 { get; set; }
        byte Unk2 { get; set; }
        byte Unk3 { get; set; }
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        ushort Unk8 { get; set; }
        public override string ToString() => $"do_script {ScriptId}";
    }
}
