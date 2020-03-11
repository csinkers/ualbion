using System;
using SerdesNet;

namespace UAlbion.Formats
{
    public class ConvertMaxToNull : IConverter<ushort, ushort?>
    {
        public static readonly ConvertMaxToNull Instance = new ConvertMaxToNull();
        ConvertMaxToNull() { }
        public static ushort? Serdes(string name, ushort? existing, Func<string, ushort, ushort> serializer) => Instance.ToMemory(serializer(name, Instance.ToPersistent(existing)));
        public ushort ToPersistent(ushort? memory) => memory ?? 0xffff;
        public ushort? ToMemory(ushort persistent) => persistent == 0xffff ? (ushort?)null : persistent;
    }
}
