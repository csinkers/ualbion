using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("change_item")]
public class ChangeItemEvent : MapEvent, IDataChangeEvent
{
    public override MapEventType EventType => MapEventType.DataChange;
    public ChangeProperty ChangeProperty => ChangeProperty.Item;
    [EventPart("target")] public TargetId Target { get; private set; }
    [EventPart("item")] public ItemId ItemId { get; private set; }
    [EventPart("op")] public NumericOperation Operation { get; private set; }
    [EventPart("amount", true, (ushort)0)] public ushort Amount { get; private set; }
    [EventPart("random", true, false)] public bool IsRandom { get; private set; }

    ChangeItemEvent() { }
    public ChangeItemEvent(TargetId target, ItemId itemId, NumericOperation operation, ushort amount, bool isRandom = false)
    {
        // change_item Target.PartyLeader Torch Add 5
        Target = target;
        ItemId = itemId;
        Operation = operation;
        Amount = amount;
        IsRandom = isRandom;
    }

    public static ChangeItemEvent Serdes(ChangeItemEvent e, AssetMapping mapping, ISerializer s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new ChangeItemEvent();

        var (targetType, targetId) = DataChangeEvent.UnpackTargetId(e.Target);
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);                               // 2
        targetType  = s.EnumU8(nameof(Target), targetType);                                   // 3
        e.IsRandom  = s.UInt8(nameof(IsRandom), (byte)(e.IsRandom ? 1 : 0)) != 0;             // 4
        targetId    = s.UInt8("TargetId", targetId);                                          // 5
        e.ItemId    = ItemId.SerdesU16(nameof(ItemId), e.ItemId, AssetType.Item, mapping, s); // 6
        e.Amount    = s.UInt16(nameof(Amount), e.Amount);                                     // 8
        e.Target    = DataChangeEvent.PackTargetId(targetType, targetId);
        return e;
    }
}
