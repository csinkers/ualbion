using System;
using System.Globalization;
using SerdesNet;

namespace UAlbion.Formats.Assets
{
    public class NpcCountTransform : IConverter<byte, int>
    {
        public static readonly NpcCountTransform Instance = new NpcCountTransform();
        NpcCountTransform() { }
        public static int Serdes(string name, int existing, Func<string, byte, byte> serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            return Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing)));
        }

        public byte ToNumeric(int memory) => memory switch
        {
            0x20 => (byte)0,
            0x60 => (byte)0x40,
            _ when memory > 0xff => throw new InvalidOperationException("Too many NPCs in map"),
            _ => (byte)memory
        };

        public int FromNumeric(byte persistent) => persistent switch
        {
            0 => 0x20,
            0x40 => 0x60,
            _ => persistent
        };

        public string ToSymbolic(int memory) => memory.ToString(CultureInfo.InvariantCulture);
        public int FromSymbolic(string symbolic) => int.Parse(symbolic, CultureInfo.InvariantCulture);
    }
}
