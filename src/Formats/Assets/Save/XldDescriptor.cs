using System;
using SerdesNet;

namespace UAlbion.Formats.Assets.Save
{
    public class XldDescriptor
    {
        public const int SizeInBytes = 8;
        public uint Size { get; set; }
        public XldCategory Category { get; set; }
        public ushort Number { get; set; }

        public static XldDescriptor Serdes(int _, XldDescriptor d, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            d ??= new XldDescriptor();
            d.Size = s.UInt32(nameof(Size), d.Size);
            d.Category = s.EnumU16(nameof(Category), d.Category);
            d.Number = s.UInt16(nameof(Number), d.Number);
            return d;
        }
    }
}
