using System;
using SerdesNet;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

[Event("modify_gold")]
public class ModifyGoldEvent : ModifyEvent
{
    public static ModifyGoldEvent Serdes(ModifyGoldEvent e, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new ModifyGoldEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        int zeroes = s.UInt8("b3", 0);
        zeroes += s.UInt8("b4", 0);
        zeroes += s.UInt8("b5", 0);
        e.Amount = s.UInt16(nameof(Amount), e.Amount);
        zeroes += s.UInt16("w8", 0);
        s.Assert(zeroes == 0, "ModifyGoldEvent expected fields 3,4,5,8 to be 0");
        return e;
    }
    ModifyGoldEvent() { }

    public ModifyGoldEvent(NumericOperation operation, ushort amount)
    {
        Operation = operation;
        Amount = amount;
    }

    [EventPart("op")] public NumericOperation Operation { get; private set; }
    [EventPart("amount")] public ushort Amount { get; private set; }
    public override ModifyType SubType => ModifyType.PartyGold;
}