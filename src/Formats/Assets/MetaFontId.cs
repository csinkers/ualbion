using System;
using UAlbion.Config;

namespace UAlbion.Formats.Assets
{
    public struct MetaFontId : IConvertible, IEquatable<MetaFontId>
    {
        public MetaFontId(bool isBold, FontColor color)
        {
            IsBold = isBold;
            Color = color;
        }

        public bool IsBold { get; }
        public FontColor Color { get; }
        public SpriteId FontId => IsBold ? Base.Font.BoldFont : Base.Font.RegularFont;

        public static explicit operator int(MetaFontId id) => (byte)id.Color << 8 | (id.IsBold ? 1 : 0);
        public static explicit operator MetaFontId(int id) => ToMetaFontId(id);

        public static explicit operator ushort(MetaFontId id) => (ushort)((byte)id.Color << 8 | (id.IsBold ? 1 : 0));
        public static explicit operator MetaFontId(ushort id) => ToMetaFontId(id);
        public static MetaFontId ToMetaFontId(int id) => new MetaFontId((id & 1) != 0, (FontColor)((id & 0xff00) >> 8));
        public static MetaFontId ToMetaFontId(ushort id) => new MetaFontId((id & 1) != 0, (FontColor)((id & 0xff00) >> 8));

        public int ToInt32(IFormatProvider provider) => (int)this;

        public TypeCode GetTypeCode() => throw new NotImplementedException();
        public bool ToBoolean(IFormatProvider provider) => throw new NotImplementedException();
        public byte ToByte(IFormatProvider provider) => throw new NotImplementedException();
        public char ToChar(IFormatProvider provider) => throw new NotImplementedException();
        public DateTime ToDateTime(IFormatProvider provider) => throw new NotImplementedException();
        public decimal ToDecimal(IFormatProvider provider) => throw new NotImplementedException();
        public double ToDouble(IFormatProvider provider) => throw new NotImplementedException();
        public short ToInt16(IFormatProvider provider) => throw new NotImplementedException();
        public long ToInt64(IFormatProvider provider) => throw new NotImplementedException();
        public sbyte ToSByte(IFormatProvider provider) => throw new NotImplementedException();
        public float ToSingle(IFormatProvider provider) => throw new NotImplementedException();
        public string ToString(IFormatProvider provider) => throw new NotImplementedException();
        public ushort ToUInt16(IFormatProvider provider) => throw new NotImplementedException();
        public uint ToUInt32(IFormatProvider provider) => throw new NotImplementedException();
        public ulong ToUInt64(IFormatProvider provider) => throw new NotImplementedException();
        public object ToType(Type conversionType, IFormatProvider provider) => throw new NotImplementedException();

        public override bool Equals(object obj) => obj is MetaFontId other && Equals(other);
        public override int GetHashCode() => (int)this;
        public static bool operator ==(MetaFontId left, MetaFontId right) => left.Equals(right);
        public static bool operator !=(MetaFontId left, MetaFontId right) => !(left == right);
        public bool Equals(MetaFontId other) => IsBold == other.IsBold && Color == other.Color;
    }
}
