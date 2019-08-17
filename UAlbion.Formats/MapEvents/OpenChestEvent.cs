using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class OpenChestEvent : MapEvent
    {
        public OpenChestEvent(BinaryReader br, int id)
        {
            throw new System.NotImplementedException();
        }

        public override EventType Type => EventType.Chest;
        public byte LockStrength { get; set; }
        public byte KeyItemId { get; set; }
        public byte Unk3 { get; set; }
        public byte ClosedMessageId { get; set; }
        public byte OpenedMessageId { get; set; }
        public ushort ChestId { get; set; }
        public ushort UnkTrapEvent { get; set; }
    }
}