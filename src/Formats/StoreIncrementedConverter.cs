using System;
using System.Globalization;
using SerdesNet;

namespace UAlbion.Formats
{
    public sealed class StoreIncrementedConverter :
        IConverter<uint, uint>,
        IConverter<int, int>,
        IConverter<ushort, ushort>,
        IConverter<short, short>,
        IConverter<byte, byte>
    {
        public static readonly StoreIncrementedConverter Instance = new StoreIncrementedConverter();
        StoreIncrementedConverter() { }
        public static byte Serdes(string name, byte existing, Func<string, byte, byte, byte> serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            return Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing), 0));
        }

        public static ushort Serdes(string name, ushort existing, Func<string, ushort, ushort, ushort> serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            return Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing), 0));
        }

        public static uint Serdes(string name, uint existing, Func<string, uint, uint, uint> serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            return Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing), 0));
        }

        public static short Serdes(string name, short existing, Func<string, short, short, short> serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            return Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing), 0));
        }

        public static int Serdes(string name, int existing, Func<string, int, int, int> serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            return Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing), 0));
        }

        public uint   FromNumeric(uint   x) => x - 1;
        public int    FromNumeric(int    x) => x - 1;
        public ushort FromNumeric(ushort x) => (ushort)(x - 1);
        public short  FromNumeric(short  x) => (short)(x - 1);
        public byte   FromNumeric(byte   x) => (byte)(x - 1);

        public uint   ToNumeric(uint x) => x + 1;
        public int    ToNumeric(int x) => x + 1;
        public ushort ToNumeric(ushort x) => (ushort)(x + 1);
        public short  ToNumeric(short x) => (short)(x + 1);
        public byte   ToNumeric(byte x) => (byte)(x + 1);

        public string ToSymbolic(uint   memory) => memory.ToString(CultureInfo.InvariantCulture);
        public string ToSymbolic(int    memory) => memory.ToString(CultureInfo.InvariantCulture);
        public string ToSymbolic(ushort memory) => memory.ToString(CultureInfo.InvariantCulture);
        public string ToSymbolic(short  memory) => memory.ToString(CultureInfo.InvariantCulture);
        public string ToSymbolic(byte   memory) => memory.ToString(CultureInfo.InvariantCulture);
        uint   IConverter<uint,     uint>.FromSymbolic(string symbolic) =>   uint.Parse(symbolic, CultureInfo.InvariantCulture);
        int    IConverter<int,       int>.FromSymbolic(string symbolic) =>    int.Parse(symbolic, CultureInfo.InvariantCulture);
        ushort IConverter<ushort, ushort>.FromSymbolic(string symbolic) => ushort.Parse(symbolic, CultureInfo.InvariantCulture);
        short  IConverter<short,   short>.FromSymbolic(string symbolic) =>  short.Parse(symbolic, CultureInfo.InvariantCulture);
        byte   IConverter<byte,     byte>.FromSymbolic(string symbolic) =>   byte.Parse(symbolic, CultureInfo.InvariantCulture);
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
        public static readonly StoreIncrementedConverter<T> Instance = new StoreIncrementedConverter<T>();
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

        public string ToSymbolic(T x) => Enum.GetName(typeof(T), x);
        public T FromSymbolic(string x) => (T)Enum.Parse(typeof(T), x);
    }
}
