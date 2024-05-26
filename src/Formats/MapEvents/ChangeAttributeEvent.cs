using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("change_attribute")]
public class ChangeAttributeEvent : MapEvent, IDataChangeEvent
{
    public override MapEventType EventType => MapEventType.DataChange;
    public ChangeProperty ChangeProperty => ChangeProperty.Attribute;
    [EventPart("target")] public TargetId Target { get; private set; }
    [EventPart("attribute")] public PhysicalAttribute Attribute { get; private set; }
    [EventPart("op")] public NumericOperation Operation { get; private set; }
    [EventPart("amount", true, (ushort)0)] public ushort Amount { get; private set; }
    [EventPart("random", true, false)] public bool IsRandom { get; private set; }

    ChangeAttributeEvent() { }
    public ChangeAttributeEvent(TargetId target, PhysicalAttribute attribute, NumericOperation operation, ushort amount = 0, bool isRandom = false)
    {
        Attribute = attribute;
        Target = target;
        Operation = operation;
        Amount = amount;
        IsRandom = isRandom;
    }

    public static ChangeAttributeEvent Serdes(ChangeAttributeEvent e, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new ChangeAttributeEvent();
        var (targetType, targetId) = DataChangeEvent.UnpackTargetId(e.Target);
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);                   // 2
        targetType  = s.EnumU8(nameof(Target), targetType);                       // 3
        e.IsRandom  = s.UInt8(nameof(IsRandom), (byte)(e.IsRandom ? 1 : 0)) != 0; // 4
        targetId    = s.UInt8("TargetId", targetId);                              // 5
        e.Attribute = s.EnumU8(nameof(Attribute), e.Attribute);                   // 6
        s.UInt8("Pad", 0);
        e.Amount    = s.UInt16(nameof(Amount), e.Amount);                         // 8
        e.Target    = DataChangeEvent.PackTargetId(targetType, targetId);
        return e;
    }
}