using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("set_chest_open")]
public class SetChestOpenEvent : ModifyEvent
{
    public static SetChestOpenEvent Serdes(SetChestOpenEvent e, AssetMapping mapping, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new SetChestOpenEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        int zeroes = s.UInt8("byte3", 0);
        zeroes += s.UInt8("byte4", 0);
        zeroes += s.UInt8("byte5", 0);
        e.Chest = ChestId.SerdesU16(nameof(Chest), e.Chest, mapping, s);
        zeroes += s.UInt16("word8", 0);
        ApiUtil.Assert(zeroes == 0, "Expected fields 3,4,5,8 to be 0 in ChestOpenEvent");
        return e;
    }

    SetChestOpenEvent() { }
    public SetChestOpenEvent(SwitchOperation operation, ChestId chest)
    {
        Operation = operation;
        Chest = chest;
    }

    [EventPart("op")] public SwitchOperation Operation { get; private set; }
    [EventPart("chest")] public ChestId Chest { get; private set; }
    public override ModifyType SubType => ModifyType.ChestOpen;
}