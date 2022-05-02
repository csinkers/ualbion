using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

[Event("change_status")]
public class ChangeStatusEvent : MapEvent, IDataChangeEvent
{
    public override MapEventType EventType => MapEventType.DataChange;
    public ChangeProperty ChangeProperty => ChangeProperty.Status;
    [EventPart("target")] public TargetId Target { get; private set; }
    [EventPart("status")] public PlayerCondition Status { get; private set; }
    [EventPart("op")] public NumericOperation Operation { get; private set; }
    [EventPart("amount", true, (ushort)0)] public ushort Amount { get; private set; }
    [EventPart("random", true, false)] public bool IsRandom { get; private set; }

    ChangeStatusEvent() { }
    public ChangeStatusEvent(TargetId target, PlayerCondition status, NumericOperation operation, ushort amount = 0, bool isRandom = false)
    {
        Status = status;
        Target = target;
        Operation = operation;
        Amount = amount;
        IsRandom = isRandom;
    }

    public static ChangeStatusEvent Serdes(ChangeStatusEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new ChangeStatusEvent();
        var (targetType, targetId) = DataChangeEvent.UnpackTargetId(e.Target);
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);                   // 2
        targetType  = s.EnumU8(nameof(Target), targetType);                       // 3
        e.IsRandom  = s.UInt8(nameof(IsRandom), (byte)(e.IsRandom ? 1 : 0)) != 0; // 4
        targetId    = s.UInt8("TargetId", targetId);                              // 5
        e.Status    = s.EnumU16(nameof(Status), e.Status);                        // 6
        e.Amount    = s.UInt16(nameof(Amount), e.Amount);                         // 8
        e.Target    = DataChangeEvent.PackTargetId(targetType, targetId);
        return e;
    }
}