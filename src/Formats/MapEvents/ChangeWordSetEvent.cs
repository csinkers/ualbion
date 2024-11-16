﻿using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("change_wordset")]
public class ChangeWordSetEvent : MapEvent, IDataChangeEvent
{
    public override MapEventType EventType => MapEventType.DataChange;
    public ChangeProperty ChangeProperty => ChangeProperty.WordSetId;
    [EventPart("target")] public TargetId Target { get; private set; }
    [EventPart("wordset")] public EventSetId WordSet { get; private set; }
    [EventPart("op")] public NumericOperation Operation { get; private set; }
    [EventPart("amount", true, (ushort)0)] public ushort Amount { get; private set; }
    [EventPart("random", true, false)] public bool IsRandom { get; private set; }

    ChangeWordSetEvent() { }
    public ChangeWordSetEvent(TargetId target, EventSetId wordset, NumericOperation operation, ushort amount = 0, bool isRandom = false)
    {
        WordSet = wordset;
        Target = target;
        Operation = operation;
        Amount = amount;
        IsRandom = isRandom;
    }

    public static ChangeWordSetEvent Serdes(ChangeWordSetEvent e, AssetMapping mapping, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new ChangeWordSetEvent();
        var (targetType, targetId) = DataChangeEvent.UnpackTargetId(e.Target);
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);                   // 2
        targetType  = s.EnumU8(nameof(Target), targetType);                       // 3
        e.IsRandom  = s.UInt8(nameof(IsRandom), (byte)(e.IsRandom ? 1 : 0)) != 0; // 4
        targetId    = s.UInt8("TargetId", targetId);                              // 5
        e.WordSet   = EventSetId.SerdesU16(nameof(EventSet), e.WordSet, mapping, s); // 6
        e.Amount    = s.UInt16(nameof(Amount), e.Amount);                         // 8
        e.Target    = DataChangeEvent.PackTargetId(targetType, targetId);
        return e;
    }
}