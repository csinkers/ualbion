using System.Diagnostics;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class SimpleChestEvent : IMapEvent
    {
        public static SimpleChestEvent Serdes(SimpleChestEvent e, ISerializer s)
        {
            e ??= new SimpleChestEvent();
            e.ChestType = s.EnumU8(nameof(ChestType), e.ChestType);
            e.Unk2 = s.UInt8(nameof(Unk2), e.Unk2);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.ItemId = s.UInt16(nameof(ItemId), e.ItemId);
            e.Amount = s.UInt16(nameof(Amount), e.Amount);
            Debug.Assert(e.Unk2 == 0);
            Debug.Assert(e.Unk3 == 0);
            Debug.Assert(e.Unk4 == 0);
            Debug.Assert(e.Unk5 == 0);
            return e;
        }
        public enum SimpleChestItemType : byte
        {
            Item = 0,
            Gold = 1, // ??
            Rations = 2 // ??
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
        public MapEventType EventType => MapEventType.SimpleChest;
    }
}
