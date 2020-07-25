using System;
using System.Globalization;
using SerdesNet;

namespace UAlbion.Formats
{
    public class ConvertMaxToNull : IConverter<ushort, ushort?>
    {
        public static readonly ConvertMaxToNull Instance = new ConvertMaxToNull();
        ConvertMaxToNull() { }
        public static ushort? Serdes(string name, ushort? existing, Func<string, ushort, ushort> serializer) => Instance.FromNumeric(serializer(name, Instance.ToNumeric(existing)));
        public ushort ToNumeric(ushort? memory) => memory ?? 0xffff;
        public ushort? FromNumeric(ushort persistent) => persistent == 0xffff ? (ushort?)null : persistent;
        public string ToSymbolic(ushort? memory) => memory?.ToString(CultureInfo.InvariantCulture);
        public ushort? FromSymbolic(string symbolic) => symbolic == null ? null : (ushort?)ushort.Parse(symbolic, CultureInfo.InvariantCulture);
    }
}
