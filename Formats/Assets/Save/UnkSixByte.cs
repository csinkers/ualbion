using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets.Save
{
    public class UnkSixByte
    {
        public byte Unk0 { get; set; }
        public byte Unk1 { get; set; }
        public byte Unk2 { get; set; }
        public byte Unk3 { get; set; }
        public byte Unk4 { get; set; }
        public byte Unk5 { get; set; }

        public override string ToString() => $"Unk {Unk0:X2} {Unk1:X2} {Unk2:X2} {Unk3:X2} {Unk4:X2} {Unk5:X2}";
        public static UnkSixByte Serdes(UnkSixByte u, ISerializer s)
        {
            u ??= new UnkSixByte();
            u.Unk0 = s.UInt8(nameof(Unk0), u.Unk0);
            u.Unk1 = s.UInt8(nameof(Unk1), u.Unk1);
            u.Unk2 = s.UInt8(nameof(Unk2), u.Unk2);
            u.Unk3 = s.UInt8(nameof(Unk3), u.Unk3);
            u.Unk4 = s.UInt8(nameof(Unk4), u.Unk4);
            u.Unk5 = s.UInt8(nameof(Unk5), u.Unk5);
            return u;
        }
    }
}