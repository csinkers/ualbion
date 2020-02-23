using System;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats
{
    public class Tweak :
        IConverter<uint, uint?>,
        IConverter<int, int?>,
        IConverter<ushort, ushort?>,
        IConverter<short, short?>,
        IConverter<byte, byte?>
    {
        public static readonly Tweak Instance = new Tweak();

        public static byte? Serdes(string name, byte? existing, Func<string, byte, byte> serializer) => Instance.ToMemory(serializer(name, Instance.ToPersistent(existing)));
        public static ushort? Serdes(string name, ushort? existing, Func<string, ushort, ushort> serializer) => Instance.ToMemory(serializer(name, Instance.ToPersistent(existing)));
        public static uint? Serdes(string name, uint? existing, Func<string, uint, uint> serializer) => Instance.ToMemory(serializer(name, Instance.ToPersistent(existing)));
        public static short? Serdes(string name, short? existing, Func<string, short, short> serializer) => Instance.ToMemory(serializer(name, Instance.ToPersistent(existing)));
        public static int? Serdes(string name, int? existing, Func<string, int, int> serializer) => Instance.ToMemory(serializer(name, Instance.ToPersistent(existing)));

        Tweak() { }
        public uint? ToMemory(uint x)
        {
            if (x == 0) return null;
            if (x < 100) return x - 1;
            return x;
        }

        public uint ToPersistent(uint? x)
        {
            if (x == null) return 0;
            if (x < 99) return x.Value + 1;
            return x.Value;
        }

        public ushort? ToMemory(ushort x)
        {
            if (x == 0) return null;
            if (x < 100) return (ushort?)(x - 1);
            return x;
        }

        public ushort ToPersistent(ushort? x)
        {
            if (x == null) return 0;
            if (x < 99) return (ushort)(x.Value + 1);
            return x.Value;
        }

        public byte? ToMemory(byte x)
        {
            if (x == 0) return null;
            if (x < 100) return (byte?)(x - 1);
            return x;
        }

        public byte ToPersistent(byte? x)
        {
            if (x == null) return 0;
            if (x < 99) return (byte)(x.Value + 1);
            return x.Value;
        }

        public int? ToMemory(int x)
        {
            if (x == 0) return null;
            if (x < 100) return x - 1;
            return x;
        }

        public int ToPersistent(int? x)
        {
            if (x == null) return 0;
            if (x < 99) return x.Value + 1;
            return x.Value;
        }

        public short? ToMemory(short x)
        {
            if (x == 0) return null;
            if (x < 100) return (short?)(x - 1);
            return x;
        }

        public short ToPersistent(short? x)
        {
            if (x == null) return 0;
            if (x < 99) return (short)(x.Value + 1);
            return x.Value;
        }
    }
}