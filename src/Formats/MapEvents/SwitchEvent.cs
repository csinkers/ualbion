using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("switch")]
public class SwitchEvent : ModifyEvent
{
    SwitchEvent() { }
    public SwitchEvent(SwitchOperation operation, SwitchId switchId, byte unk3)
    {
        Operation = operation;
        SwitchId = switchId;
        Unk3 = unk3;
    }

    public static SwitchEvent Serdes(SwitchEvent e, AssetMapping mapping, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new SwitchEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
        int zeroed = s.UInt8(null, 0);
        zeroed += s.UInt8(null, 0);
        e.SwitchId = SwitchId.SerdesU16(nameof(SwitchId), e.SwitchId, mapping, s);
        zeroed += s.UInt16(null, 0);
        s.Assert(zeroed == 0, "SwitchEvent: Expected fields 4,5,8 to be 0");
        return e;
    }

    [EventPart("op")] public SwitchOperation Operation { get; private set; } // 0,1,2
    [EventPart("switch")] public SwitchId SwitchId { get; private set; } // [0..599]
    [EventPart("unk3", true, (byte)0)] public byte Unk3 { get; private set; } // 0,1,21
    public override ModifyType SubType => ModifyType.Switch;
}