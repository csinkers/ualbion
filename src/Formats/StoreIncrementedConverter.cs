using System;
using System.Globalization;
using SerdesNet;

namespace UAlbion.Formats;

public sealed class StoreIncrementedConverter :
    IConverter<uint, uint>,
    IConverter<int, int>,
    IConverter<ushort, ushort>,
    IConverter<short, short>,
    IConverter<byte, byte>
{
    public static readonly StoreIncrementedConverter Instance = new();
    StoreIncrementedConverter() { }
    public static byte Serdes(string name, byte existing, Func<string, byte, byte, byte> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        return Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing), 0));
    }

    public static ushort Serdes(string name, ushort existing, Func<string, ushort, ushort, ushort> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        return Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing), 0));
    }

    public static uint Serdes(string name, uint existing, Func<string, uint, uint, uint> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        return Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing), 0));
    }

    public static short Serdes(string name, short existing, Func<string, short, short, short> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        return Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing), 0));
    }

    public static int Serdes(string name, int existing, Func<string, int, int, int> serializer)
    {
        ArgumentNullException.ThrowIfNull(serializer);
        return Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing), 0));
    }

    public uint   FromNumeric(uint   persistent) => persistent - 1;
    public int    FromNumeric(int    persistent) => persistent - 1;
    public ushort FromNumeric(ushort persistent) => (ushort)(persistent - 1);
    public short  FromNumeric(short  persistent) => (short)(persistent - 1);
    public byte   FromNumeric(byte   persistent) => (byte)(persistent - 1);

    public uint   ToNumeric(uint memory) => memory + 1;
    public int    ToNumeric(int memory) => memory + 1;
    public ushort ToNumeric(ushort memory) => (ushort)(memory + 1);
    public short  ToNumeric(short memory) => (short)(memory + 1);
    public byte   ToNumeric(byte memory) => (byte)(memory + 1);

    public string ToSymbolic(uint   memory) => memory.ToString(CultureInfo.InvariantCulture);
    public string ToSymbolic(int    memory) => memory.ToString(CultureInfo.InvariantCulture);
    public string ToSymbolic(ushort memory) => memory.ToString(CultureInfo.InvariantCulture);
    public string ToSymbolic(short  memory) => memory.ToString(CultureInfo.InvariantCulture);
    public string ToSymbolic(byte   memory) => memory.ToString(CultureInfo.InvariantCulture);
    uint   IConverter<uint,     uint>.FromSymbolic(string symbolic) =>   uint.Parse(symbolic);
    int    IConverter<int,       int>.FromSymbolic(string symbolic) =>    int.Parse(symbolic);
    ushort IConverter<ushort, ushort>.FromSymbolic(string symbolic) => ushort.Parse(symbolic);
    short  IConverter<short,   short>.FromSymbolic(string symbolic) =>  short.Parse(symbolic);
    byte   IConverter<byte,     byte>.FromSymbolic(string symbolic) =>   byte.Parse(symbolic);
}

public sealed class StoreIncrementedConverter<T> :
    IConverter<byte, T>,
    IConverter<sbyte, T>,
    IConverter<ushort, T>,
    IConverter<short, T>,
    IConverter<uint, T>,
    IConverter<int, T>,
    IConverter<ulong, T>,
    IConverter<long, T>
    where T : struct, Enum
{
    public static readonly StoreIncrementedConverter<T> Instance = new();
    StoreIncrementedConverter() { }
    T IConverter<  byte, T>.FromNumeric(  byte x) => (T)(object)(  byte)(x - 1);
    T IConverter< sbyte, T>.FromNumeric( sbyte x) => (T)(object)( sbyte)(x - 1);
    T IConverter<ushort, T>.FromNumeric(ushort x) => (T)(object)(ushort)(x - 1);
    T IConverter< short, T>.FromNumeric( short x) => (T)(object)( short)(x - 1);
    T IConverter<  uint, T>.FromNumeric(  uint x) => (T)(object)(x - 1);
    T IConverter<   int, T>.FromNumeric(   int x) => (T)(object)(x - 1);
    T IConverter< ulong, T>.FromNumeric( ulong x) => (T)(object)(x - 1);
    T IConverter<  long, T>.FromNumeric(  long x) => (T)(object)(x - 1);

    byte   IConverter<  byte, T>.ToNumeric(T x) => (  byte)((  byte)(object)x + 1);
    sbyte  IConverter< sbyte, T>.ToNumeric(T x) => ( sbyte)(( sbyte)(object)x + 1);
    ushort IConverter<ushort, T>.ToNumeric(T x) => (ushort)((ushort)(object)x + 1);
    short  IConverter< short, T>.ToNumeric(T x) => ( short)(( short)(object)x + 1);
    uint   IConverter<  uint, T>.ToNumeric(T x) => ( uint)(object)x + 1;
    int    IConverter<   int, T>.ToNumeric(T x) => (  int)(object)x + 1;
    ulong  IConverter< ulong, T>.ToNumeric(T x) => (ulong)(object)x + 1;
    long   IConverter<  long, T>.ToNumeric(T x) => ( long)(object)x + 1;

    public string ToSymbolic(T memory) => Enum.GetName(typeof(T), memory);
    public T FromSymbolic(string symbolic) => (T)Enum.Parse(typeof(T), symbolic);
}
