using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class TeleportEvent : MapEvent
    {
        public TeleportEvent(BinaryReader br, int id)
        {
            throw new System.NotImplementedException();
        }

        public override EventType Type => EventType.MapExit;
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte DirectionX { get; set; }
        public byte Always255 { get; set; }
        public byte Unk5 { get; set; }
        public ushort MapId { get; set; }
    }
}