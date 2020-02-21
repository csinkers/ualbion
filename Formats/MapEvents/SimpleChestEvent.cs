using System.Diagnostics;
using System.IO;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class SimpleChestEvent : IMapEvent
    {
        public static SimpleChestEvent Translate(SimpleChestEvent e, ISerializer s)
        {
            e ??= new SimpleChestEvent();
            s.EnumU8(nameof(ChestType), () => e.ChestType, x => e.ChestType = x, x => ((byte)x, x.ToString()));
            s.Dynamic(e, nameof(Unk2));
            s.Dynamic(e, nameof(Unk3));
            s.Dynamic(e, nameof(Unk4));
            s.Dynamic(e, nameof(Unk5));
            s.Dynamic(e, nameof(ItemId));
            s.Dynamic(e, nameof(Amount));
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
