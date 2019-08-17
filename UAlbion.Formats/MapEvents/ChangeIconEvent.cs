using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class ChangeIconEvent : MapEvent
    {
        public ChangeIconEvent(BinaryReader br, int id)
        {
            X = br.ReadByte();
            Y = br.ReadByte();
            Permanent = br.ReadByte();
            ChangeType = (IconChangeType) br.ReadByte();
            Unk5 = br.ReadByte();
            Value = br.ReadUInt16();
        }

        public enum IconChangeType : byte
        {
            ChangeUnderlay = 0,
            ChangeOverlay = 1,
            ChangeWall = 2,
            ChangeFloor = 3,
            ChangeCeiling = 4,
            ChangeNpcMovementType = 5,
            ChangeNpcSprite = 6,
            ChangeTileEventChain = 7,
            PlaceTilemapObjectOverwrite = 8, // Objects are in BLKLIST#.XLD
            PlaceTilemapObjectNoOverwrite = 9, // Objects are in BLKLIST#.XLD
            ChangeTileEventTrigger = 0xA, // ???? Might not be 0xA
        }
        public override EventType Type => EventType.ChangeIcon;
        public byte X { get; }
        public byte Y { get; }
        public byte Permanent { get; }
        public IconChangeType ChangeType { get; }
        public byte Unk5 { get; }
        public ushort Value { get; }
    }
}