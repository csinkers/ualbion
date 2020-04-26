using SerdesNet;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class OpenChestEvent : MapEvent
    {
        public static OpenChestEvent Serdes(OpenChestEvent e, ISerializer s)
        {
            e ??= new OpenChestEvent();
            e.LockStrength = s.UInt8(nameof(LockStrength), e.LockStrength);
            e.KeyItemId = (ItemId?)StoreIncrementedNullZero.Serdes(nameof(KeyItemId), (ushort?)e.KeyItemId, s.UInt16);
            e.ClosedMessageId = s.UInt8(nameof(ClosedMessageId), e.ClosedMessageId);
            e.OpenedMessageId = s.UInt8(nameof(OpenedMessageId), e.OpenedMessageId);
            e.ChestId = s.UInt16(nameof(ChestId), e.ChestId);
            e.TrapEvent = s.UInt16(nameof(TrapEvent), e.TrapEvent);
            return e;
        }

        public byte LockStrength { get; set; }
        public ItemId? KeyItemId { get; set; }
        public byte ClosedMessageId { get; set; }
        public byte OpenedMessageId { get; set; }
        public ushort ChestId { get; set; }
        public ushort TrapEvent { get; set; }
        public override string ToString() => $"open_chest {ChestId} Trap:{TrapEvent} Key:{KeyItemId} Lock:{LockStrength} Opened:{OpenedMessageId} Closed:{ClosedMessageId}";
        public override MapEventType EventType => MapEventType.Chest;
    }
}
