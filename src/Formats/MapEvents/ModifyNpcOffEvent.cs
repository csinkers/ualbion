using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("modify_npc_off")]
public class ModifyNpcOffEvent : ModifyEvent, INpcEvent
{
    ModifyNpcOffEvent() { }

    public ModifyNpcOffEvent(SwitchOperation operation, byte npcNum, MapId map)
    {
        NpcNum = npcNum;
        Operation = operation;
        Map = map;
    }

    public static ModifyNpcOffEvent Serdes(ModifyNpcOffEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new ModifyNpcOffEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.NpcNum = s.UInt8(nameof(NpcNum), e.NpcNum);
        int zeroed = s.UInt8("byte4", 0);
        zeroed += s.UInt8("byte5", 0);
        e.Map = MapId.SerdesU16(nameof(Map), e.Map, mapping, s);
        zeroed += s.UInt16("word8", 0);
        s.Assert(zeroed == 0, "ModifyNpcOffEvent: Expected fields 4,5,8 to be 0");
        return e;
    }

    [EventPart("op")] public SwitchOperation Operation { get; private set; }
    [EventPart("npcNum")] public byte NpcNum { get; private set; }
    [EventPart("map", true, "None")] public MapId Map { get; private set; }
    public override ModifyType SubType => ModifyType.NpcOff;
}