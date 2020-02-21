using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class OpenChestEvent : IMapEvent
    {
        public static OpenChestEvent Translate(OpenChestEvent node, ISerializer s)
        {
            node ??= new OpenChestEvent();
            s.Dynamic(node, nameof(LockStrength));
            s.Dynamic(node, nameof(KeyItemId));
            s.Dynamic(node, nameof(Unk3));
            s.Dynamic(node, nameof(ClosedMessageId));
            s.Dynamic(node, nameof(OpenedMessageId));
            s.Dynamic(node, nameof(ChestId));
            s.Dynamic(node, nameof(TrapEvent));
            return node;
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
