using System.Diagnostics;
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
            var chestEvent = new SimpleChestEvent
            {
                ChestType = (SimpleChestItemType) br.ReadByte(), // +1
                Unk2 = br.ReadByte(), // +2
                Unk3 = br.ReadByte(), // +3
                Unk4 = br.ReadByte(), // +4
                Unk5 = br.ReadByte(), // +5
                ItemId = br.ReadUInt16(), // +6
                Amount = br.ReadUInt16(), // +8
            };
            Debug.Assert(chestEvent.Unk2 == 0);
            Debug.Assert(chestEvent.Unk3 == 0);
            Debug.Assert(chestEvent.Unk4 == 0);
            Debug.Assert(chestEvent.Unk5 == 0);
            return new EventNode(id, chestEvent);
        }

        public SimpleChestItemType ChestType { get; private set; }
        public ushort ItemId { get; private set; }
        public ushort Amount { get; private set; }
        byte Unk2 { get; set; }
        byte Unk3 { get; set; }
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }

        string ItemIdString =>
            ChestType switch
            {
                SimpleChestItemType.Item => ((ItemId)ItemId).ToString(),
                _ => ItemId.ToString()
            };

        public override string ToString() => $"simple_chest {ChestType} {Amount}x{ItemIdString}";
    }
}
