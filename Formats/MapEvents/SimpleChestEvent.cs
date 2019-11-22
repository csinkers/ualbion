using System.IO;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class SimpleChestEvent : IEvent
    {
        public enum SimpleChestItemType : byte
        {
            Item = 0,
            Gold = 1, // ??
            Rations = 2 // ??
        }

        public static EventNode Load(BinaryReader br, int id, MapEventType type)
        {
            return new EventNode(id, new SimpleChestEvent
            {
                ChestType = (SimpleChestItemType) br.ReadByte(), // +1
                Unk2 = br.ReadByte(), // +2
                Unk3 = br.ReadByte(), // +3
                Unk4 = br.ReadByte(), // +4
                Unk5 = br.ReadByte(), // +5
                ItemId = br.ReadUInt16(), // +6
                Amount = br.ReadUInt16(), // +8
            });
        }

        public SimpleChestItemType ChestType { get; private set; }
        public ushort ItemId { get; private set; }
        public ushort Amount { get; private set; }
        public byte Unk2 { get; private set; }
        public byte Unk3 { get; private set; }
        public byte Unk4 { get; private set; }
        public byte Unk5 { get; private set; }

        string ItemIdString =>
            ChestType switch
            {
                SimpleChestItemType.Item => ((ItemId)ItemId).ToString(),
                _ => ItemId.ToString()
            };

        public override string ToString() => $"simple_chest {ChestType} {Amount}x{ItemIdString} ({Unk2} {Unk3} {Unk4} {Unk5})";
    }
}
