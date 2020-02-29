using UAlbion.Api;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class ChangeIconEvent : Event, IPositionedEvent, IMapEvent
    {
        public static ChangeIconEvent Serdes(ChangeIconEvent e, ISerializer s)
        {
            e ??= new ChangeIconEvent();
            e.X = s.Int8(nameof(X), e.X);
            e.Y = s.Int8(nameof(Y), e.Y);
            e.Scope = s.EnumU8(nameof(Scope), e.Scope);
            e.ChangeType = s.EnumU8(nameof(ChangeType), e.ChangeType);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Value = s.UInt16(nameof(Value), e.Value);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            return e;
        }

        public enum IconChangeType : byte
        {
            ChangeUnderlay = 0,
            ChangeOverlay = 1,
            ChangeWall = 2,
            ChangeFloor = 3,
            ChangeCeiling = 4,
            ChangeNpcMovementType = 5, // X = NpcId, Values: 0=Waypoints, 1=Random, 2=Stay, 3=Follow
            ChangeNpcSprite = 6, // X = NpcId
            ChangeTileEventChain = 7,
            PlaceTilemapObjectOverwrite = 8, // Objects are in BLKLIST#.XLD
            PlaceTilemapObjectNoOverwrite = 9, // Objects are in BLKLIST#.XLD
            ChangeTileEventTrigger = 0xA, // ???? Might not be 0xA
        }

        public IPositionedEvent OffsetClone(int x, int y) =>
            new ChangeIconEvent
            {
                X = (sbyte) (X + x),
                Y = (sbyte) (Y + y),
                Scope = Scope,
                ChangeType = ChangeType,
                Unk5 = Unk5,
                Value = Value,
                Unk8 = Unk8
            };

        int IPositionedEvent.X => X;
        int IPositionedEvent.Y => Y;
        public sbyte X { get; set; }
        public sbyte Y { get; set; }
        public EventScope Scope { get; set; }
        public IconChangeType ChangeType { get; set; }
        public byte Unk5 { get; set; }
        public ushort Value { get; set; }
        public ushort Unk8 { get; set; }
        public override string ToString() => $"change_icon <{X}, {Y}> {Scope} {ChangeType} {Value} ({Unk5} {Unk8})";
        public MapEventType EventType => MapEventType.ChangeIcon;
    }
}
