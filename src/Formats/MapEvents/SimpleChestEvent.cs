using System;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    [Event("simple_chest", "Pickup some items from the map", new[] { "sc" })]
    public class SimpleChestEvent : MapEvent
    {
        public SimpleChestEvent(ItemId item, ushort amount)
        {
            ChestType = item.Type switch 
                {
                    AssetType.Gold => SimpleChestItemType.Gold,
                    AssetType.Rations => SimpleChestItemType.Rations,
                    _ => SimpleChestItemType.Item
                };
            ItemId = item;
            Amount = amount;
        }

        SimpleChestEvent() { }

        public static SimpleChestEvent Serdes(SimpleChestEvent e, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new SimpleChestEvent();
            e.ChestType = s.EnumU8(nameof(ChestType), e.ChestType);
            uint padding = s.UInt32(nameof(padding), 0);
            ApiUtil.Assert(padding == 0);
            e.ItemId = ItemId.SerdesU16(nameof(e.ItemId), e.ItemId, AssetType.Item, mapping, s);
            e.Amount = s.UInt16(nameof(Amount), e.Amount);
            return e;
        }

        public enum SimpleChestItemType : byte
        {
            Item = 0,
            Gold = 1, // ??
            Rations = 2 // ??
        }

        public SimpleChestItemType ChestType { get; private set; }
        [EventPart("item")] public ItemId ItemId { get; private set; }
        [EventPart("amount")] public ushort Amount { get; private set; }

        public override string ToString() => $"simple_chest {ChestType} {Amount}x{ItemId}";
        public override MapEventType EventType => MapEventType.SimpleChest;
    }
}
