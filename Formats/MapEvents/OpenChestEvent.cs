using SerdesNet;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public class OpenChestEvent : MapEvent, ITextEvent
    {
        OpenChestEvent(AssetType textType, ushort textSourceId)
        {
            TextType = textType;
            TextSourceId = textSourceId;
        }

        public static OpenChestEvent Serdes(OpenChestEvent e, ISerializer s, AssetType textType, ushort textSourceId)
        {
            e ??= new OpenChestEvent(textType, textSourceId);
            e.LockStrength = s.UInt8(nameof(LockStrength), e.LockStrength);
            e.KeyItemId = (ItemId?)StoreIncrementedNullZero.Serdes(nameof(KeyItemId), (ushort?)e.KeyItemId, s.UInt16);
            e.ClosedMessageId = s.UInt8(nameof(ClosedMessageId), e.ClosedMessageId);
            e.OpenedMessageId = s.UInt8(nameof(OpenedMessageId), e.OpenedMessageId);
            e.ChestId = s.EnumU16(nameof(ChestId), e.ChestId);
            e.TrapEvent = s.UInt16(nameof(TrapEvent), e.TrapEvent);
            return e;
        }

        public byte LockStrength { get; private set; }
        public ItemId? KeyItemId { get; private set; }
        public byte ClosedMessageId { get; private set; }
        public byte OpenedMessageId { get; private set; }
        public ChestId ChestId { get; private set; }
        public ushort TrapEvent { get; private set; }
        public override string ToString() => $"open_chest {ChestId} Trap:{TrapEvent} Key:{KeyItemId} Lock:{LockStrength} Opened:{OpenedMessageId} Closed:{ClosedMessageId}";
        public override MapEventType EventType => MapEventType.Chest;
        public AssetType TextType { get; }
        public ushort TextSourceId { get; }
    }
}
