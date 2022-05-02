using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

[Event("modify_item_count")]
public class ModifyItemCountEvent : ModifyEvent
{
    public static ModifyItemCountEvent Serdes(ModifyItemCountEvent e, AssetMapping mapping, ISerializer s)
    {
        if (s == null) throw new ArgumentNullException(nameof(s));
        e ??= new ModifyItemCountEvent();
        e.Operation = s.EnumU8(nameof(Operation), e.Operation);
        e.Amount = s.UInt8(nameof(Amount), e.Amount);
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.ItemId = ItemId.SerdesU16(nameof(e.ItemId), e.ItemId, AssetType.Item, mapping, s);
        zeroes += s.UInt16(null, 0);
        s.Assert(zeroes == 0, "AddRemoveInventoryItem: Expected fields 4,5 & 8 to be 0");
        return e;
    }

    ModifyItemCountEvent() { }
    public ModifyItemCountEvent(NumericOperation operation, byte amount, ItemId itemId)
    {
        Operation = operation;
        Amount = amount;
        ItemId = itemId;
    }

    [EventPart("operation")] public NumericOperation Operation { get; private set; }
    [EventPart("amount")] public byte Amount { get; private set; }
    [EventPart("item_id")] public ItemId ItemId { get; private set; }

    public override ModifyType SubType => ModifyType.ItemCount;
}