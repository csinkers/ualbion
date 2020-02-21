using UAlbion.Api;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.MapEvents
{
    public class ChangeIconEvent : IPositionedEvent, IMapEvent
    {
        public static ChangeIconEvent Translate(ChangeIconEvent node, ISerializer s)
        {
            node ??= new ChangeIconEvent();
            s.Dynamic(node, nameof(X));
            s.Dynamic(node, nameof(Y));
            s.Dynamic(node, nameof(Permanent));
            s.EnumU8(nameof(ChangeType), () => node.ChangeType, x => node.ChangeType = x, x => ((byte) x, x.ToString()));
            s.Dynamic(node, nameof(Unk5));
            s.Dynamic(node, nameof(Value));
            s.Dynamic(node, nameof(Unk8));
            return node;
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

        public IPositionedEvent OffsetClone(int x, int y) =>
            new ChangeIconEvent
            {
                X = (sbyte) (X + x),
                Y = (sbyte) (Y + y),
                Permanent = Permanent,
                ChangeType = ChangeType,
                Unk5 = Unk5,
                Value = Value,
                Unk8 = Unk8
            };

        int IPositionedEvent.X => X;
        int IPositionedEvent.Y => Y;
        public sbyte X { get; set; }
        public sbyte Y { get; set; }
        public byte Permanent { get; set; }
        public IconChangeType ChangeType { get; set; }
        public byte Unk5 { get; set; }
        public ushort Value { get; set; }
        public ushort Unk8 { get; set; }
        public override string ToString() => $"change_icon <{X}, {Y}> {(Permanent != 0 ? "Perm" : "Temp")} {ChangeType} {Value} ({Unk5} {Unk8})";
        public MapEventType EventType => MapEventType.ChangeIcon;
    }
}
