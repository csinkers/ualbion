using System;

namespace UAlbion.Formats.AssetIds
{
    public enum FontColor
    {
        White = 1,
        Yellow = 2,
        YellowOrange = 6
        /* Inks:
        1: Regular white
        2: Yellow
        6: Yellow/orange
        +64: Damaged
        ??: Gray
        */,
        Gray
    }

    public class MetaFontId : IConvertible
    {
        public MetaFontId(bool isBold = false, FontColor color = FontColor.White)
        {
            IsBold = isBold;
            Color = color;
        }

        public bool IsBold { get; }
        public FontColor Color { get; }
        public FontId FontId => IsBold ? FontId.BoldFont : FontId.RegularFont;

        public static explicit operator int(MetaFontId id) => (int)id.Color << 8 | (id.IsBold ? 1 : 0);

        public static explicit operator MetaFontId(int id) => new MetaFontId((id & 1) != 0, (FontColor)((id & 0xff00) >> 8));

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
    }
}
