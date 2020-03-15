using SerdesNet;

namespace UAlbion.Formats.Assets.Save
{
    public class UnkEightByte
    {
        public enum Enum1 : byte
        {
            Unk0,
            Unk1,
            Unk2,
            Unk3,
            Unk4,
            Unk5,
            Unk6,
            Unk7,
            Unk8,
            Unk9,
        }

        public enum Enum2 : byte
        {
            Common,
            Rare1,
            Rare2,
            Norm,
        }

        public byte X { get; set; } // Broad, mildly lower-biased distribution. Max below 255 (~220)
        public byte Y { get; set; } // Broad, mildly lower-biased distribution. Max below 255 (~220)
        public Enum1 Unk2 { get; set; } // Roughly equal distribution over [0..10], most likely an enum.
        public Enum2 Unk3 { get; set; } // Ranges over [0..3], 3 very popular, 0 moderately, 1 and 2 ~1% each.
        public ushort Underlay { get; set; }
        public ushort Overlay { get; set; }

        public override string ToString() => $"Unk {X:X2} {Y:X2} {Unk2:X2} {Unk3:X2} {Underlay:X6} {Overlay:X6}";
        public static UnkEightByte Serdes(UnkEightByte u, ISerializer s)
        {
            u ??= new UnkEightByte();
            u.X = s.UInt8(nameof(X), u.X);
            u.Y = s.UInt8(nameof(Y), u.Y);
            u.Unk2 = s.EnumU8(nameof(Unk2), u.Unk2);
            u.Unk3 = s.EnumU8(nameof(Unk3), u.Unk3);
            u.Underlay = s.UInt16(nameof(Underlay), u.Underlay);
            u.Overlay = s.UInt16(nameof(Overlay), u.Overlay);
            return u;
        }
    }
}
