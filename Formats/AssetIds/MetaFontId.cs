using System;

namespace UAlbion.Formats.AssetIds
{
    public enum CommonColor : byte
    {
        Black1        = 192, // #000000 
        Black2        = 193, // #000000 
        White         = 194, // #ffffff 
        BlueGrey7     = 195, // #e7dfd7 
        BlueGrey6     = 196, // #c3bfbb 
        BlueGrey5     = 197, // #9b9fa3 
        BlueGrey4     = 198, // #777f8f 
        BlueGrey3     = 199, // #575f73 
        BlueGrey2     = 200, // #373f53 
        BlueGrey1     = 201, // #171f37 
        Lavender      = 202, // #9b8bb7 
        Purple        = 203, // #73639f 
        ReddishBlue   = 204, // #3b4383 
        BluishRed     = 205, // #97737f 
        LightBurgundy = 206, // #8b4b63 
        Burgundy      = 207, // #77274f 
        Orange5       = 208, // #e79713 
        Orange4       = 209, // #c36300 
        Orange3       = 210, // #a73f00 
        Orange2       = 211, // #7f1f00 
        Orange1       = 212, // #5f1300 
        Green6        = 213, // #a7a723 
        Green5        = 214, // #77832f 
        Green4        = 215, // #436323 
        Green3        = 216, // #1f4727 
        Green2        = 217, // #002b27 
        Green1        = 218, // #00171b 
        Yellow5       = 219, // #ffe373 
        Yellow4       = 220, // #e7c333 
        Yellow3       = 221, // #cf9f2b 
        Yellow2       = 222, // #b37b13 
        Yellow1       = 223, // #835313 
        Teal4         = 224, // #77afbb 
        Teal3         = 225, // #438f8f 
        Teal2         = 226, // #23676b 
        Teal1         = 227, // #134343 
        Blue4         = 228, // #3b8fb3 
        Blue3         = 229, // #236b9b 
        Blue2         = 230, // #0f4b7f 
        Blue1         = 231, // #003757 
        Flesh8        = 232, // #e3bb8f 
        Flesh7        = 233, // #c39767 
        Flesh6        = 234, // #ab774b 
        Flesh5        = 235, // #8f5b33 
        Flesh4        = 236, // #77431f 
        Flesh3        = 237, // #5b2f0f 
        Flesh2        = 238, // #3f1b07 
        Flesh1        = 239, // #2b1300 
        Grey1         = 240, // #170b00 
        Grey2         = 241, // #1f130b 
        Grey3         = 242, // #1f1b13 
        Grey4         = 243, // #27231f 
        Grey5         = 244, // #2b2b27 
        Grey6         = 245, // #333333 
        Grey7         = 246, // #3b3b3b 
        Grey8         = 247, // #434343 
        Grey9         = 248, // #4b4b47 
        Grey10        = 249, // #57534b 
        Grey11        = 250, // #5f5b57 
        Grey12        = 251, // #67635f 
        Grey13        = 252, // #736f6b 
        Grey14        = 253, // #7b7b77 
        Grey15        = 254, // #878b8b 
        MidGrey       = 255  // #5b5363 
    }

    public enum FontColor
    {
        White,
        Yellow,
        Red
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
