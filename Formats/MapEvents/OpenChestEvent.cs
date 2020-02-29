using UAlbion.Api;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class OpenChestEvent : Event, IMapEvent
    {
        public static OpenChestEvent Serdes(OpenChestEvent e, ISerializer s)
        {
            e ??= new OpenChestEvent();
            e.LockStrength = s.UInt8(nameof(LockStrength), e.LockStrength);
            e.KeyItemId = s.UInt8(nameof(KeyItemId), e.KeyItemId);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.ClosedMessageId = s.UInt8(nameof(ClosedMessageId), e.ClosedMessageId);
            e.OpenedMessageId = s.UInt8(nameof(OpenedMessageId), e.OpenedMessageId);
            e.ChestId = s.UInt16(nameof(ChestId), e.ChestId);
            e.TrapEvent = s.UInt16(nameof(TrapEvent), e.TrapEvent);
            return e;
        }

        public byte LockStrength { get; set; }
        public byte KeyItemId { get; set; }
        public byte Unk3 { get; set; }
        public byte ClosedMessageId { get; set; }
        public byte OpenedMessageId { get; set; }
        public ushort ChestId { get; set; }
        public ushort TrapEvent { get; set; }
        public override string ToString() => $"open_chest {ChestId} Trap:{TrapEvent} Key:{KeyItemId} Lock:{LockStrength} Opened:{OpenedMessageId} Closed:{ClosedMessageId} ({Unk3})";
        public MapEventType EventType => MapEventType.Chest;
    }
}
