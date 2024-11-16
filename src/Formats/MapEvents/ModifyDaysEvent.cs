﻿using System;
using SerdesNet;
using UAlbion.Api.Eventing;

namespace UAlbion.Formats.MapEvents;

[Event("modify_days")]
public class ModifyDaysEvent : ModifyEvent
{
    public static ModifyDaysEvent Serdes(ModifyDaysEvent e, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new ModifyDaysEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.Amount = s.UInt16(nameof(Amount), e.Amount);
        zeroes += s.UInt16(null, 0);
        s.Assert(zeroes == 0, "ChangeTime: Expected fields 3,4,5,8 to be 0");
        return e;
    }

    ModifyDaysEvent(){}
    public ModifyDaysEvent(NumericOperation operation, ushort amount)
    {
        Operation = operation;
        Amount = amount;
    }

    [EventPart("op")] public NumericOperation Operation { get; private set; }
    [EventPart("amount")] public ushort Amount { get; private set; }
    public override ModifyType SubType => ModifyType.TimeDays;
}