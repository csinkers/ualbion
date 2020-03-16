using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    [Event("change_icon")]
    public class ChangeIconEvent : MapEvent
    {
        public static ChangeIconEvent Serdes(ChangeIconEvent e, ISerializer s)
        {
            e ??= new ChangeIconEvent();
            e.X = s.Int8(nameof(X), (sbyte)e.X);
            e.Y = s.Int8(nameof(Y), (sbyte)e.Y);
            e.Scope = s.EnumU8(nameof(Scope), e.Scope);
            e.ChangeType = s.EnumU8(nameof(ChangeType), e.ChangeType);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Value = s.UInt16(nameof(Value), e.Value);
            // e.Value = StoreIncremented.Serdes(nameof(Value), e.Value, s.UInt16);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            ApiUtil.Assert(e.Unk5 == 0
                    || e.Unk5 == 1
                    || e.Unk5 == 2
                    || e.Unk5 == 3);
            ApiUtil.Assert(e.Unk8 == 0); // Is 152 for a single change wall event in the endgame. Probably just an oversight.
            return e;
        }

        ChangeIconEvent() { }
        public ChangeIconEvent(short x, short y, EventScope scope, IconChangeType changeType, ushort value)
        {
            X = x;
            Y = y;
            Scope = scope;
            ChangeType = changeType;
            Value = value;
        }

        public enum IconChangeType : byte
        {
            Underlay = 0,
            Overlay = 1,
            Wall = 2,
            Floor = 3,
            Ceiling = 4,
            NpcMovement = 5, // X = NpcId, Values: 0=Waypoints, 1=Random, 2=Stay, 3=Follow
            NpcSprite = 6, // X = NpcId
            Chain = 7,
            BlockHard = 8, // Objects are in BLKLIST#.XLD (overwrite existing tiles)
            BlockSoft = 9, // Objects are in BLKLIST#.XLD (don't overwrite)
            Trigger = 0xA, // ???? Might not be 0xA
        }

        [EventPart("x")] public short X { get; private set; }
        [EventPart("y")] public short Y { get; private set; }
        [EventPart("scope")] public EventScope Scope { get; private set; }
        [EventPart("type")] public IconChangeType ChangeType { get; private set; }
        [EventPart("value")] public ushort Value { get; private set; }
        public byte Unk5 { get; private set; }
        ushort Unk8 { get; set; }
        // public override string ToString() => $"change_icon <{X}, {Y}> ({Scope}) {ChangeType} {Value} ({Unk5} {Unk8})";
        public override MapEventType EventType => MapEventType.ChangeIcon;
    }
}
