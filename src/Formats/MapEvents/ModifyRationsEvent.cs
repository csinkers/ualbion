using System;
using SerdesNet;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

[Event("modify_rations")]
public class ModifyRationsEvent : ModifyEvent
{
    public static ModifyRationsEvent Serdes(ModifyRationsEvent e, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new ModifyRationsEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);

        int zeroes = s.UInt8("b4", 0);
        if (zeroes != 0)
            s.Assert(false, $"ModifyRationsEvent expected field 4 to be 0, but it was {zeroes}");

        zeroes = s.UInt8("b5", 0);
        if (zeroes != 0)
            s.Assert(false, $"ModifyRationsEvent expected field 5 to be 0, but it was {zeroes}");

        e.Amount = s.UInt16(nameof(Amount), e.Amount);

        zeroes = s.UInt16("w8", 0);
        if (zeroes != 0)
            s.Assert(false, $"ModifyRationsEvent expected field 8 to be 0, but it was {zeroes}");

        return e;
    }

    ModifyRationsEvent() { }

    public ModifyRationsEvent(NumericOperation operation, ushort amount, byte unk3)
    {
        Operation = operation;
        Amount = amount;
        Unk3 = unk3;
    }

    [EventPart("op")] public NumericOperation Operation { get; private set; }
    [EventPart("amount")] public ushort Amount { get; private set; }
    [EventPart("unk3", true, (byte)0)] public byte Unk3 { get; private set; }
    public override ModifyType SubType => ModifyType.PartyRations;
}
