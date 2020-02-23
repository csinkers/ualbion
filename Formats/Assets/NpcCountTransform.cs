using System;
using UAlbion.Formats.Parsers;

namespace UAlbion.Formats.Assets
{
    public class NpcCountTransform : IConverter<byte, int>
    {
        public static readonly NpcCountTransform Instance = new NpcCountTransform();
        NpcCountTransform() { }
        public static int Serdes(string name, int existing, Func<string, byte, byte> serializer) => Instance.ToMemory(serializer(name, Instance.ToPersistent(existing)));
        public byte ToPersistent(int memory) => memory switch
        {
            0x20 => (byte)0,
            0x60 => (byte)0x40,
            _ when memory > 0xff => throw new InvalidOperationException("Too many NPCs in map"),
            _ => (byte)memory
        };

        public int ToMemory(byte persistent) => persistent switch
        {
            0 => 0x20,
            0x40 => 0x60,
            _ => persistent
        };
    }
}