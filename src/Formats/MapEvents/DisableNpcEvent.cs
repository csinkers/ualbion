using System;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents;

[Event("disable_npc")]
public class DisableNpcEvent : ModifyEvent, INpcEvent
{
    DisableNpcEvent() { }

    public DisableNpcEvent(byte npcNum, byte isDisabled, byte unk5, ushort unk6)
    {
        NpcNum = npcNum;
        IsDisabled = isDisabled;
        Unk5 = unk5;
        Unk6 = unk6;
    }

    public static DisableNpcEvent Serdes(DisableNpcEvent e, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new DisableNpcEvent();
        e.IsDisabled = s.UInt8(nameof(IsDisabled), e.IsDisabled);
        e.NpcNum = s.UInt8(nameof(NpcNum), e.NpcNum);
        int zeroed = s.UInt8(null, 0);
        e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
        e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
        zeroed += s.UInt16(null, 0);
        s.Assert(zeroed == 0, "DisableNpcEvent:Expected fields 4,8 to be 0");
        return e;
    }

    [EventPart("npcNum")] public byte NpcNum { get; private set; }
    [EventPart("disable", true, (byte)1)] public byte IsDisabled { get; private set; }
    [EventPart("unk5", true, (byte)0)] public byte Unk5 { get; private set; }
    [EventPart("unk6", true, (ushort)0)] public ushort Unk6 { get; private set; }
    public override ModifyType SubType => ModifyType.NpcActive;
}