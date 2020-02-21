using System.Diagnostics;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class DoScriptEvent : IMapEvent
    {
        public static DoScriptEvent Translate(DoScriptEvent e, ISerializer s)
        {
            e ??= new DoScriptEvent();
            s.Dynamic(e, nameof(Unk1));
            s.Dynamic(e, nameof(Unk2));
            s.Dynamic(e, nameof(Unk3));
            s.Dynamic(e, nameof(Unk4));
            s.Dynamic(e, nameof(Unk5));
            s.Dynamic(e, nameof(ScriptId));
            s.Dynamic(e, nameof(Unk8));
            Debug.Assert(e.Unk1 == 0);
            Debug.Assert(e.Unk2 == 0);
            Debug.Assert(e.Unk3 == 0);
            Debug.Assert(e.Unk4 == 0);
            Debug.Assert(e.Unk5 == 0);
            Debug.Assert(e.Unk8 == 0);
            return e;
        }

        public ushort ScriptId { get; private set; }
        byte Unk1 { get; set; }
        byte Unk2 { get; set; }
        byte Unk3 { get; set; }
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        ushort Unk8 { get; set; }
        public override string ToString() => $"do_script {ScriptId}";
        public MapEventType EventType => MapEventType.DoScript;
    }
}
