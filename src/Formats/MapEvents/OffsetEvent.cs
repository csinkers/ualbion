using System;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public class OffsetEvent : MapEvent
    {
        public static OffsetEvent Serdes(OffsetEvent e, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            e ??= new OffsetEvent();
            e.X = s.Int8(nameof(X), e.X);
            e.Y = s.Int8(nameof(Y), e.Y);
            e.Unk3 = s.UInt8(nameof(Unk3), e.Unk3);
            e.Unk4 = s.UInt8(nameof(Unk4), e.Unk4);
            e.Unk5 = s.UInt8(nameof(Unk5), e.Unk5);
            e.Unk6 = s.UInt16(nameof(Unk6), e.Unk6);
            e.Unk8 = s.UInt16(nameof(Unk8), e.Unk8);
            ApiUtil.Assert(e.Unk3 == 1 || e.Unk3 == 3);
            ApiUtil.Assert(e.Unk4 == 0);
            ApiUtil.Assert(e.Unk5 == 0);
            ApiUtil.Assert(e.Unk6 == 0);
            ApiUtil.Assert(e.Unk8 == 0);
            return e;
        }

        public sbyte X { get; private set; }
        public sbyte Y { get; private set; }
        public byte Unk3 { get; private set; }
        byte Unk4 { get; set; }
        byte Unk5 { get; set; }
        ushort Unk6 { get; set; }
        ushort Unk8 { get; set; }
        public override string ToString() => $"offset <{X}, {Y}> ({Unk3} {Unk4} {Unk5} {Unk6} {Unk8})";
        public override MapEventType EventType => MapEventType.Offset;
    }
}
