using SerdesNet;

namespace UAlbion.Formats.Assets.Save
{
    public class XldDescriptor
    {

        public uint Size { get; set; }
        public XldCategory Category { get; set; }
        public ushort Number { get; set; }

        public static XldDescriptor Serdes(int _, XldDescriptor d, ISerializer s)
        {
            d ??= new XldDescriptor();
            s.Begin();
            d.Size = s.UInt32(nameof(Size), d.Size);
            d.Category = s.EnumU16(nameof(Category), d.Category);
            d.Number = s.UInt16(nameof(Number), d.Number);
            s.End();
            return d;
        }
    }
}
