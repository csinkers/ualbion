﻿using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("door_open")]
public class DoorOpenEvent : ModifyEvent
{
    public static DoorOpenEvent Serdes(DoorOpenEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new DoorOpenEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        int zeroes = s.UInt8("byte3", 0);
        zeroes += s.UInt8("byte4", 0);
        zeroes += s.UInt8("byte5", 0);
        e.Door = DoorId.SerdesU16(nameof(Door), e.Door, mapping, s);
        zeroes += s.UInt16("word8", 0);
        ApiUtil.Assert(zeroes == 0, "Expected fields 3,4,5,8 to be 0 in DoorOpenEvent");
        return e;
    }

    DoorOpenEvent() { }
    public DoorOpenEvent(SwitchOperation operation, DoorId door)
    {
        Operation = operation;
        Door = door;
    }

    [EventPart("op")] public SwitchOperation Operation { get; private set; }
    [EventPart("door")] public DoorId Door { get; private set; }
    public override ModifyType SubType => ModifyType.DoorOpen;
}