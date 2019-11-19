using System.IO;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class SimpleChestEvent : MapEvent
    {
        public enum SimpleChestItemType : byte
        {
            Item = 0,
            Gold = 1, // ??
            Rations = 2 // ??
        }

        public SimpleChestEvent(BinaryReader br, int id, EventType type) : base(id, type)
        {
            ChestType = (SimpleChestItemType)br.ReadByte(); // +1
            Unk2 = br.ReadByte(); // +2
            Unk3 = br.ReadByte(); // +3
            Unk4 = br.ReadByte(); // +4
            Unk5 = br.ReadByte(); // +5
            ItemId = br.ReadUInt16(); // +6
            Amount = br.ReadUInt16(); // +8
        }

        public SimpleChestItemType ChestType { get; }
        public ushort ItemId { get; }
        public ushort Amount { get; }
        public byte Unk2 { get; }
        public byte Unk3 { get; }
        public byte Unk4 { get; }
        public byte Unk5 { get; }

        string ItemIdString =>
            ChestType switch
            {
                SimpleChestItemType.Item => ((ItemId)ItemId).ToString(),
                _ => ItemId.ToString()
            };

        public override string ToString() => $"simple_chest {ChestType} {Amount}x{ItemIdString} ({Unk2} {Unk3} {Unk4} {Unk5})";
    }
}