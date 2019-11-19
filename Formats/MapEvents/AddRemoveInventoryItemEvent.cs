using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class AddRemoveInventoryItemEvent : ModifyEvent
    {
        public AddRemoveInventoryItemEvent(BinaryReader br, int id, EventType type, ModifyType subType) : base(id, type, subType)
        {
            Operation = (QuantityChangeOperation)br.ReadByte(); // 2
            Amount = br.ReadByte(); // 3
            Unk4 = br.ReadByte(); // 4
            Unk5 = br.ReadByte(); // 5
            ItemId = br.ReadUInt16(); // 6
            Unk8 = br.ReadUInt16(); // 8
        }

        public QuantityChangeOperation Operation { get; }
        public byte Amount { get; set; }
        public ushort ItemId { get; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }
        public ushort Unk8 { get; set; }
        public override string ToString() => $"add_remove_inv_item {Operation} {Amount}x{ItemId} ({Unk4} {Unk5} {Unk8})";
    }
}