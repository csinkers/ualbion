using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    [Event("do_script")]
    public class DoScriptEvent : AsyncMapEvent
    {
        DoScriptEvent() { }
        public DoScriptEvent(ScriptId scriptId) => ScriptId = scriptId;

        public static DoScriptEvent Serdes(DoScriptEvent e, ISerializer s)
        {
            e ??= new DoScriptEvent();
            e.Unk1 = s.UInt8(nameof(Unk1), e.Unk1);
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.ScriptId = (ScriptId)StoreIncremented.Serdes(nameof(ScriptId), (ushort)e.ScriptId, s.UInt16);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            ApiUtil.Assert(e.Unk1 == 0);
            ApiUtil.Assert(e.Unk2 == 0);
            ApiUtil.Assert(e.Unk3 == 0);
            ApiUtil.Assert(e.Unk4 == 0);
            ApiUtil.Assert(e.Unk5 == 0);
            ApiUtil.Assert(e.Unk8 == 0);
            return e;
        }

        [EventPart("id")] public ScriptId ScriptId { get; private set; }
        byte Unk1 { get; set; }
        byte Unk2 { get; set; }
        byte Unk3 { get; set; }
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        ushort Unk8 { get; set; }
        public override MapEventType EventType => MapEventType.DoScript;
    }
}
