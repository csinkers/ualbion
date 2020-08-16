using System;
using System.Globalization;
using SerdesNet;

namespace UAlbion.Formats
{
    public sealed class StoreIncrementedNullZero :
        IConverter<  byte,   byte?>,
        IConverter< sbyte,  sbyte?>,
        IConverter<ushort, ushort?>,
        IConverter< short,  short?>,
        IConverter<  uint,   uint?>,
        IConverter<   int,    int?>,
        IConverter< ulong,  ulong?>,
        IConverter<  long,   long?>
    {
        public static readonly StoreIncrementedNullZero Instance = new StoreIncrementedNullZero();
        StoreIncrementedNullZero() { }
        public   byte ToNumeric(  byte? x) =>   (byte)(x + 1 ?? 0);
        public  sbyte ToNumeric( sbyte? x) =>  (sbyte)(x + 1 ?? 0);
        public ushort ToNumeric(ushort? x) => (ushort)(x + 1 ?? 0);
        public  short ToNumeric( short? x) =>  (short)(x + 1 ?? 0);
        public   uint ToNumeric(  uint? x) => x + 1 ?? 0;
        public    int ToNumeric(   int? x) => x + 1 ?? 0;
        public  ulong ToNumeric( ulong? x) => x + 1 ?? 0;
        public   long ToNumeric(  long? x) => x + 1 ?? 0;

        public   byte? FromNumeric(  byte x) => x == 0 ? null :   (byte?)(x - 1);
        public  sbyte? FromNumeric( sbyte x) => x == 0 ? null :  (sbyte?)(x - 1);
        public ushort? FromNumeric(ushort x) => x == 0 ? null : (ushort?)(x - 1);
        public  short? FromNumeric( short x) => x == 0 ? null :  (short?)(x - 1);
        public   uint? FromNumeric(  uint x) => x == 0 ? null :   (uint?)(x - 1);
        public    int? FromNumeric(   int x) => x == 0 ? null :    (int?)(x - 1);
        public  ulong? FromNumeric( ulong x) => x == 0 ? null :  (ulong?)(x - 1);
        public   long? FromNumeric(  long x) => x == 0 ? null :   (long?)(x - 1);

        public string ToSymbolic(  byte? x) => x == null ? null : x.ToString();
        public string ToSymbolic( sbyte? x) => x == null ? null : x.ToString();
        public string ToSymbolic(ushort? x) => x == null ? null : x.ToString();
        public string ToSymbolic( short? x) => x == null ? null : x.ToString();
        public string ToSymbolic(  uint? x) => x == null ? null : x.ToString();
        public string ToSymbolic(   int? x) => x == null ? null : x.ToString();
        public string ToSymbolic( ulong? x) => x == null ? null : x.ToString();
        public string ToSymbolic(  long? x) => x == null ? null : x.ToString();

        byte?   IConverter<byte,     byte?>.FromSymbolic(string x) => x == null ?   (byte?)null :   byte.Parse(x, CultureInfo.InvariantCulture);
        sbyte?  IConverter<sbyte,   sbyte?>.FromSymbolic(string x) => x == null ?  (sbyte?)null :  sbyte.Parse(x, CultureInfo.InvariantCulture);
        ushort? IConverter<ushort, ushort?>.FromSymbolic(string x) => x == null ? (ushort?)null : ushort.Parse(x, CultureInfo.InvariantCulture);
        short?  IConverter<short,   short?>.FromSymbolic(string x) => x == null ?  (short?)null :  short.Parse(x, CultureInfo.InvariantCulture);
        uint?   IConverter<uint,     uint?>.FromSymbolic(string x) => x == null ?   (uint?)null :   uint.Parse(x, CultureInfo.InvariantCulture);
        int?    IConverter<int,       int?>.FromSymbolic(string x) => x == null ?    (int?)null :    int.Parse(x, CultureInfo.InvariantCulture);
        ulong?  IConverter<ulong,   ulong?>.FromSymbolic(string x) => x == null ?  (ulong?)null :  ulong.Parse(x, CultureInfo.InvariantCulture);
        long?   IConverter<long,     long?>.FromSymbolic(string x) => x == null ?   (long?)null :   long.Parse(x, CultureInfo.InvariantCulture);
    }

    public sealed class StoreIncrementedNullZero<T> :
        IConverter<byte, T?>,
        IConverter<sbyte, T?>,
        IConverter<ushort, T?>,
        IConverter<short, T?>,
        IConverter<uint, T?>,
        IConverter<int, T?>,
        IConverter<ulong, T?>,
        IConverter<long, T?>
        where T : struct, Enum
    {
        public static readonly StoreIncrementedNullZero<T> Instance = new StoreIncrementedNullZero<T>();
        StoreIncrementedNullZero() { }
        T? IConverter<  byte, T?>.FromNumeric(  byte x) => x == 0 ? null : (T?)(T)(object)(  byte)(x - 1);
        T? IConverter< sbyte, T?>.FromNumeric( sbyte x) => x == 0 ? null : (T?)(T)(object)( sbyte)(x - 1);
        T? IConverter<ushort, T?>.FromNumeric(ushort x) => x == 0 ? null : (T?)(T)(object)(ushort)(x - 1);
        T? IConverter< short, T?>.FromNumeric( short x) => x == 0 ? null : (T?)(T)(object)( short)(x - 1);
        T? IConverter<  uint, T?>.FromNumeric(  uint x) => x == 0 ? null : (T?)(T)(object)(x - 1);
        T? IConverter<   int, T?>.FromNumeric(   int x) => x == 0 ? null : (T?)(T)(object)(x - 1);
        T? IConverter< ulong, T?>.FromNumeric( ulong x) => x == 0 ? null : (T?)(T)(object)(x - 1);
        T? IConverter<  long, T?>.FromNumeric(  long x) => x == 0 ? null : (T?)(T)(object)(x - 1);

        byte   IConverter<  byte, T?>.ToNumeric(T? x) => x == null ? (  byte)0 : (  byte)((  byte)(object)x.Value + 1);
        sbyte  IConverter< sbyte, T?>.ToNumeric(T? x) => x == null ? ( sbyte)0 : ( sbyte)(( sbyte)(object)x.Value + 1);
        ushort IConverter<ushort, T?>.ToNumeric(T? x) => x == null ? (ushort)0 : (ushort)((ushort)(object)x.Value + 1);
        short  IConverter< short, T?>.ToNumeric(T? x) => x == null ? ( short)0 : ( short)(( short)(object)x.Value + 1);
        uint   IConverter<  uint, T?>.ToNumeric(T? x) => x == null ? 0 : ( uint)(object)x.Value + 1;
        int    IConverter<   int, T?>.ToNumeric(T? x) => x == null ? 0 : (  int)(object)x.Value + 1;
        ulong  IConverter< ulong, T?>.ToNumeric(T? x) => x == null ? 0 : (ulong)(object)x.Value + 1;
        long   IConverter<  long, T?>.ToNumeric(T? x) => x == null ? 0 : ( long)(object)x.Value + 1;

        public string ToSymbolic(T? x) => x == null ? null : Enum.GetName(typeof(T), x.Value);
        public T? FromSymbolic(string x) => x == null ? null : (T?)(T)Enum.Parse(typeof(T), x);

    }
}
