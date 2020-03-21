using System;
using SerdesNet;

namespace UAlbion.Formats
{
    public class StoreIncrementedNullZero :
        IConverter<uint, uint?>,
        IConverter<int, int?>,
        IConverter<ushort, ushort?>,
        IConverter<short, short?>,
        IConverter<byte, byte?>
    {
        public static readonly StoreIncrementedNullZero Instance = new StoreIncrementedNullZero();
        StoreIncrementedNullZero() { }
        public static byte? Serdes(string name, byte? existing, Func<string, byte, byte> serializer) => Instance.ToMemory(serializer(name, Instance.ToPersistent(existing)));
        public static ushort? Serdes(string name, ushort? existing, Func<string, ushort, ushort> serializer) => Instance.ToMemory(serializer(name, Instance.ToPersistent(existing)));
        public static uint? Serdes(string name, uint? existing, Func<string, uint, uint> serializer) => Instance.ToMemory(serializer(name, Instance.ToPersistent(existing)));
        public static short? Serdes(string name, short? existing, Func<string, short, short> serializer) => Instance.ToMemory(serializer(name, Instance.ToPersistent(existing)));
        public static int? Serdes(string name, int? existing, Func<string, int, int> serializer) => Instance.ToMemory(serializer(name, Instance.ToPersistent(existing)));
        public uint? ToMemory(uint x) => x == 0 ? (uint?)null : x - 1;
        public uint ToPersistent(uint? x) => x + 1 ?? 0;
        public ushort? ToMemory(ushort x) => x == 0 ? null : (ushort?)(x - 1);
        public ushort ToPersistent(ushort? x) => (ushort)(x + 1 ?? 0);
        public byte? ToMemory(byte x) => x == 0 ? null : (byte?)(x - 1);
        public byte ToPersistent(byte? x) => (byte)(x + 1 ?? 0);
        public int? ToMemory(int x) => x == 0 ? null : (int?)(x - 1);
        public int ToPersistent(int? x) => x + 1 ?? 0;
        public short? ToMemory(short x) => x == 0 ? null : (short?)(x - 1);
        public short ToPersistent(short? x) => (short)(x + 1 ?? 0);
    }
}