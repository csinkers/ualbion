using System;
using System.Globalization;
using SerdesNet;

namespace UAlbion.Formats
{
    public sealed class StoreIncremented :
        IConverter<uint, uint>,
        IConverter<int, int>,
        IConverter<ushort, ushort>,
        IConverter<short, short>,
        IConverter<byte, byte>
    {
        public static readonly StoreIncremented Instance = new StoreIncremented();
        StoreIncremented() { }
        public static byte Serdes(string name, byte existing, Func<string, byte, byte> serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            return Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing)));
        }

        public static ushort Serdes(string name, ushort existing, Func<string, ushort, ushort> serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            return Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing)));
        }

        public static uint Serdes(string name, uint existing, Func<string, uint, uint> serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            return Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing)));
        }

        public static short Serdes(string name, short existing, Func<string, short, short> serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            return Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing)));
        }

        public static int Serdes(string name, int existing, Func<string, int, int> serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            return Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing)));
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
}
