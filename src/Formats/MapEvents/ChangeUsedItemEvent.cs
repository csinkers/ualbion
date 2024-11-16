using System;
using SerdesNet;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

[Event("change_used_item")]
public class ChangeUsedItemEvent : MapEvent
{
    public static ChangeUsedItemEvent Serdes(ChangeUsedItemEvent e, AssetMapping mapping, ISerdes s)
    {
        ArgumentNullException.ThrowIfNull(s);
        e ??= new ChangeUsedItemEvent();
        int zeroes = s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        zeroes += s.UInt8(null, 0);
        e.ItemId = ItemId.SerdesU16(nameof(ItemId), e.ItemId, AssetType.Item, mapping, s);
        zeroes += s.UInt16(null, 0);
        s.Assert(zeroes == 0, "ChangeUsedItem: Expected all fields other than 6 to be 0");
        return e;
    }

    ChangeUsedItemEvent() { }
    public ChangeUsedItemEvent(ItemId itemId) => ItemId = itemId;
    [EventPart("id")] public ItemId ItemId { get; private set; }
    public override MapEventType EventType => MapEventType.ChangeUsedItem;
}