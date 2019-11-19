using System.IO;

namespace UAlbion.Formats.MapEvents
{
    public class ChangeIconEvent : MapEvent
    {
        public ChangeIconEvent(BinaryReader br, int id, EventType type) : base(id, type)
        {
            X = br.ReadSByte(); // 1
            Y = br.ReadSByte(); // 2
            Permanent = br.ReadByte(); // 3
            ChangeType = (IconChangeType) br.ReadByte(); // 4
            Unk5 = br.ReadByte(); // 5
            Value = br.ReadUInt16(); // 6
            Unk8 = br.ReadUInt16();
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

        public sbyte X { get; }
        public sbyte Y { get; }
        public byte Permanent { get; }
        public IconChangeType ChangeType { get; }
        public byte Unk5 { get; }
        public ushort Value { get; }
        public ushort Unk8 { get; set; }
        public override string ToString() => $"change_icon <{X}, {Y}> {(Permanent != 0 ? "Perm" : "Temp")} {ChangeType} {Value} ({Unk5} {Unk8})";
    }
}