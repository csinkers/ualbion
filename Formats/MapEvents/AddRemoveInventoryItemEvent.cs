using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    [Event("add_remove_inv")]
    public class AddRemoveInventoryItemEvent : ModifyEvent
    {
        public static AddRemoveInventoryItemEvent Translate(AddRemoveInventoryItemEvent e, ISerializer s)
        {
            e ??= new AddRemoveInventoryItemEvent();
            s.EnumU8(nameof(Operation), 
                () => e.Operation,
                x => e.Operation = x,
                    x => ((byte)x, x.ToString()));

            s.Dynamic(e, nameof(Amount));
            s.Dynamic(e, nameof(Unk4));
            s.Dynamic(e, nameof(Unk5));
            s.UInt16(nameof(ItemId),
                () => (ushort)e.ItemId,
                x => e.ItemId = (ItemId)x);
            s.Dynamic(e, nameof(Unk8));
            return e;
        }

        AddRemoveInventoryItemEvent() { }
        public AddRemoveInventoryItemEvent(QuantityChangeOperation operation, byte amount, ItemId itemId)
        {
            Operation = operation;
            Amount = amount;
            ItemId = itemId;
        }

        [EventPart("operation")] public QuantityChangeOperation Operation { get; private set; }
        [EventPart("amount")] public byte Amount { get; private set; }
        [EventPart("item_id")] public ItemId ItemId { get; private set; }

        public byte Unk4 { get; private set; }
        public byte Unk5 { get; private set; }
        public ushort Unk8 { get; private set; }
        public override string ToString() => $"add_remove_inv_item {Operation} {Amount}x{ItemId} ({Unk4} {Unk5} {Unk8})";
        public override ModifyType SubType => ModifyType.AddRemoveInventoryItem;
    }
}
