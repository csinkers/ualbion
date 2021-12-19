using System;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    [Event("change_icon")]
    public class ChangeIconEvent : MapEvent
    {
        public static ChangeIconEvent Serdes(ChangeIconEvent e, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new ChangeIconEvent();
            e.X = s.Int8(nameof(X), (sbyte)e.X);
            e.Y = s.Int8(nameof(Y), (sbyte)e.Y);
            e.Scopes = s.EnumU8(nameof(Scopes), e.Scopes);
            e.ChangeType = s.EnumU8(nameof(ChangeType), e.ChangeType);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Value = s.UInt16(nameof(Value), e.Value);
            // e.Value = StoreIncremented.Serdes(nameof(Value), e.Value, s.UInt16);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            ApiUtil.Assert(e.Unk5 is 0 or 1 or 2 or 3);

            // Is 152 for a single change wall event in the endgame. Probably just an oversight.
            ApiUtil.Assert(e.Unk8 is 0 or 152, $"Unexpected unk8 in change_icon: {e.Unk8}");
            return e;
        }

        ChangeIconEvent() { }
        public ChangeIconEvent(short x, short y, EventScopes scopes, IconChangeType changeType, ushort value, byte unk5)
        {
            X = x;
            Y = y;
            Scopes = scopes;
            ChangeType = changeType;
            Value = value;
            Unk5 = unk5;
        }

        [EventPart("x")] public short X { get; private set; }
        [EventPart("y")] public short Y { get; private set; }
        [EventPart("scopes")] public EventScopes Scopes { get; private set; }
        [EventPart("type")] public IconChangeType ChangeType { get; private set; }
        [EventPart("value")] public ushort Value { get; private set; }
        [EventPart("unk5", true, (byte)3)] public byte Unk5 { get; private set; }
        ushort Unk8 { get; set; }
        public override MapEventType EventType => MapEventType.ChangeIcon;
    }
}
