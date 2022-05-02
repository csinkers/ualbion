using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

[Event("chest_open")]
public class ChestOpenEvent : ModifyEvent
{
    public static ChestOpenEvent Serdes(ChestOpenEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new ChestOpenEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        int zeroes = s.UInt8("byte3", 0);
        zeroes += s.UInt8("byte4", 0);
        zeroes += s.UInt8("byte5", 0);
        e.Chest = ChestId.SerdesU16(nameof(Chest), e.Chest, mapping, s);
        zeroes += s.UInt16("word8", 0);
        ApiUtil.Assert(zeroes == 0, "Expected fields 3,4,5,8 to be 0 in ChestOpenEvent");
        return e;
    }

    ChestOpenEvent() { }
    public ChestOpenEvent(SwitchOperation operation, ChestId chest)
    {
        Operation = operation;
        Chest = chest;
    }

    [EventPart("op")] public SwitchOperation Operation { get; private set; }
    [EventPart("chest")] public ChestId Chest { get; private set; }
    public override ModifyType SubType => ModifyType.ChestOpen;
}