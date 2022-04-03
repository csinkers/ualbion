using System;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents;

[Event("modify_rations")]
public class ModifyRationsEvent : ModifyEvent
{
    public static ModifyRationsEvent Serdes(ModifyRationsEvent e, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new ModifyRationsEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        int zeroes = s.UInt8("b3", 0);
        zeroes += s.UInt8("b4", 0);
        zeroes += s.UInt8("b5", 0);
        e.Amount = s.UInt16(nameof(Amount), e.Amount);
        zeroes += s.UInt16("w8", 0);
        s.Assert(zeroes == 0, "ModifyRationsEvent expected fields 3,4,5,8 to be 0");
        return e;
    }
    ModifyRationsEvent() { }

    public ModifyRationsEvent(NumericOperation operation, ushort amount)
    {
        Operation = operation;
        Amount = amount;
    }

    [EventPart("op")] public NumericOperation Operation { get; private set; }
    [EventPart("amount")] public ushort Amount { get; private set; }
    public override ModifyType SubType => ModifyType.PartyRations;
}
