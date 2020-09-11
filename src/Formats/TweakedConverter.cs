using System;
using System.Globalization;
using SerdesNet;

#pragma warning disable CA1822 // Mark members as static
namespace UAlbion.Formats
{
    public sealed class TweakedConverter :
        IConverter<uint, uint?>,
        //IConverter<int, int?>,
        IConverter<ushort, ushort?>,
        //IConverter<short, short?>,
        IConverter<byte, byte?>
    {
        public static readonly TweakedConverter Instance = new TweakedConverter();
/*
        public static byte? Serdes(string name, byte? existing, Func<string, byte, byte> serializer) => Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing)));
        public static ushort? Serdes(string name, ushort? existing, Func<string, ushort, ushort> serializer) => Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing)));
        public static uint? Serdes(string name, uint? existing, Func<string, uint, uint> serializer) => Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing)));
        public static short? Serdes(string name, short? existing, Func<string, short, short> serializer) => Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing)));
        public static int? Serdes(string name, int? existing, Func<string, int, int> serializer) => Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing)));
    */

        TweakedConverter() { }
        public uint? FromNumeric(uint x)
        {
            if (x == 0) return null;
            if (x < 100) return x - 1;
            return x;
        }

        public uint ToNumeric(uint? x)
        {
            if (x == null) return 0;
            if (x < 99) return x.Value + 1;
            return x.Value;
        }

        public ushort? FromNumeric(ushort x)
        {
            if (x == 0) return null;
            if (x < 100) return (ushort?)(x - 1);
            return x;
        }

        public ushort ToNumeric(ushort? x)
        {
            if (x == null) return 0;
            if (x < 99) return (ushort)(x.Value + 1);
            return x.Value;
        }

        public byte? FromNumeric(byte x)
        {
            if (x == 0) return null;
            if (x < 100) return (byte?)(x - 1);
            return x;
        }

        public byte ToNumeric(byte? x)
        {
            if (x == null) return 0;
            if (x < 99) return (byte)(x.Value + 1);
            return x.Value;
        }

        public int? FromNumeric(int x)
        {
            if (x == 0) return null;
            if (x < 100) return x - 1;
            return x;
        }

        public int ToNumeric(int? x)
        {
            if (x == null) return 0;
            if (x < 99) return x.Value + 1;
            return x.Value;
        }

        public short? FromNumeric(short x)
        {
            if (x == 0) return null;
            if (x < 100) return (short?)(x - 1);
            return x;
        }

        public short ToNumeric(short? x)
        {
            if (x == null) return 0;
            if (x < 99) return (short)(x.Value + 1);
            return x.Value;
        }

        public string ToSymbolic(uint?   memory) => memory?.ToString(CultureInfo.InvariantCulture);
        public string ToSymbolic(int?    memory) => memory?.ToString(CultureInfo.InvariantCulture);
        public string ToSymbolic(ushort? memory) => memory?.ToString(CultureInfo.InvariantCulture);
        public string ToSymbolic(short?  memory) => memory?.ToString(CultureInfo.InvariantCulture);
        public string ToSymbolic(byte?   memory) => memory?.ToString(CultureInfo.InvariantCulture);
        uint?   IConverter<uint,     uint?>.FromSymbolic(string symbolic) => symbolic == null ? null :   (uint?)uint.Parse(symbolic, CultureInfo.InvariantCulture);
        //int?    IConverter<int,       int?>.FromSymbolic(string symbolic) => symbolic == null ? null :    (int?)int.Parse(symbolic, CultureInfo.InvariantCulture);
        ushort? IConverter<ushort, ushort?>.FromSymbolic(string symbolic) => symbolic == null ? null : (ushort?)ushort.Parse(symbolic, CultureInfo.InvariantCulture);
        //short?  IConverter<short,   short?>.FromSymbolic(string symbolic) => symbolic == null ? null :  (short?)short.Parse(symbolic, CultureInfo.InvariantCulture);
        byte?   IConverter<byte,     byte?>.FromSymbolic(string symbolic) => symbolic == null ? null :   (byte?)byte.Parse(symbolic, CultureInfo.InvariantCulture);
    }

    public sealed class TweakedConverter<T> :
        IConverter<byte, T?>,
        //IConverter<sbyte, T?>,
        IConverter<ushort, T?>,
        //IConverter<short, T?>,
        IConverter<uint, T?>,
        //IConverter<int, T?>,
        IConverter<ulong, T?>//,
        //IConverter<long, T?>
        where T : struct, Enum
    {
        public static readonly TweakedConverter<T> Instance = new TweakedConverter<T>();
        TweakedConverter() { }
        T? IConverter<  byte, T?>.FromNumeric(  byte x) => x == 0 ? null : x < 100 ? (T?)(T)(object)(  byte)(x - 1) : (T)(object)x;
        //T? IConverter< sbyte, T?>.FromNumeric( sbyte x) => x == 0 ? null : x < 100 ? (T?)(T)(object)( sbyte)(x - 1) : (T)(object)x;
        T? IConverter<ushort, T?>.FromNumeric(ushort x) => x == 0 ? null : x < 100 ? (T?)(T)(object)(ushort)(x - 1) : (T)(object)x;
        //T? IConverter< short, T?>.FromNumeric( short x) => x == 0 ? null : x < 100 ? (T?)(T)(object)( short)(x - 1) : (T)(object)x;
        T? IConverter<  uint, T?>.FromNumeric(  uint x) => x == 0 ? null : x < 100 ? (T?)(T)(object)(x - 1) : (T)(object)x;
        //T? IConverter<   int, T?>.FromNumeric(   int x) => x == 0 ? null : x < 100 ? (T?)(T)(object)(   int)(x - 1) : (T)(object)x;
        T? IConverter< ulong, T?>.FromNumeric( ulong x) => x == 0 ? null : x < 100 ? (T?)(T)(object)(x - 1) : (T)(object)x;
        //T? IConverter<  long, T?>.FromNumeric(  long x) => x == 0 ? null : x < 100 ? (T?)(T)(object)(  long)(x - 1) : (T)(object)x;

        public uint ToNumeric(uint? x)
        {
            if (x == null) return 0;
            if (x < 99) return x.Value + 1;
            return x.Value;
        }
        byte IConverter<byte, T?>.ToNumeric(T? x)
        {
            if (x == null) return 0;
            var n = (byte)(object)x.Value;
            return n < 99 ? (byte)(n + 1) : n;
        }

        ushort IConverter<ushort, T?>.ToNumeric(T? x)
        {
            if (x == null) return 0;
            var n = (ushort)(object)x.Value;
            return n < 99 ? (ushort)(n + 1) : n;
        }

        uint IConverter<uint, T?>.ToNumeric(T? x)
        {
            if (x == null) return 0;
            var n = (uint)(object)x.Value;
            return n < 99 ? n + 1 : n;
        }

        ulong IConverter<ulong, T?>.ToNumeric(T? x)
        {
            if (x == null) return 0;
            var n = (ulong)(object)x.Value;
            return n < 99 ? n + 1 : n;
        }

        public string ToSymbolic(T? x) => x == null ? null : Enum.GetName(typeof(T), x.Value);
        public T? FromSymbolic(string x) => x == null ? null : (T?)(T)Enum.Parse(typeof(T), x);
    }
}
#pragma warning restore CA1822 // Mark members as static
