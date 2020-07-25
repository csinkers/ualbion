using SerdesNet;
using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    [Event("simple_chest", "Pickup some items from the map")]
    public class SimpleChestEvent : MapEvent
    {
        public SimpleChestEvent(SimpleChestItemType type, ItemId item, ushort amount)
        {
            ChestType = type;
            ItemId = item;
            Amount = amount;
        }

        SimpleChestEvent() { }

        public static SimpleChestEvent Serdes(SimpleChestEvent e, ISerializer s)
        {
            e ??= new SimpleChestEvent();
            s.Begin();
            e.ChestType = s.EnumU8(nameof(ChestType), e.ChestType);
            uint padding = s.UInt32(nameof(padding), 0);
            ApiUtil.Assert(padding == 0);
            e.ItemId = (ItemId)StoreIncremented.Serdes(nameof(e.ItemId), (ushort)e.ItemId, s.UInt16);
            e.Amount = s.UInt16(nameof(Amount), e.Amount);
            s.End();
            return e;
        }

        public enum SimpleChestItemType : byte
        {
            Item = 0,
            Gold = 1, // ??
            Rations = 2 // ??
        }

        [EventPart("type", "Can be Item, Gold or Rations")]
        public SimpleChestItemType ChestType { get; private set; }
        [EventPart("item")] public ItemId ItemId { get; private set; }
        [EventPart("amount")] public ushort Amount { get; private set; }

        public override string ToString() => $"simple_chest {ChestType} {Amount}x{ItemId}";
        public override MapEventType EventType => MapEventType.SimpleChest;
    }
}
