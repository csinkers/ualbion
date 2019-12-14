using System.IO;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    [Event("add_remove_inv")]
    public class AddRemoveInventoryItemEvent : ModifyEvent
    {
        public static EventNode Load(BinaryReader br, int id, MapEventType type, ModifyType subType)
        {
            return new EventNode(id, new AddRemoveInventoryItemEvent
            {
                Operation = (QuantityChangeOperation) br.ReadByte(), // 2
                Amount = br.ReadByte(), // 3
                Unk4 = br.ReadByte(), // 4
                Unk5 = br.ReadByte(), // 5
                ItemId = (ItemId)br.ReadUInt16(), // 6
                Unk8 = br.ReadUInt16(), // 8
            });
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
    }
}
