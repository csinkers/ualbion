using System;
using SerdesNet;

namespace UAlbion.Formats.Assets
{
    public class Block
    {
        int[] _underlay;
        int[] _overlay;
        public byte Width { get; set; }
        public byte Height { get; set; }
        public int GetUnderlay(int index) => _underlay[index];
        public int GetOverlay(int index) => _overlay[index];
        public override string ToString() => $"BLK {Width}x{Height}";

        public byte[] RawLayout
        {
            get => FormatUtil.ToPacked(Width, Height, _underlay, _overlay);
            set => (_underlay, _overlay) = FormatUtil.FromPacked(Width, Height, value);
        }

        public static Block Serdes(int _, Block b, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            b ??= new Block();
            b.Width = s.UInt8(nameof(Width), b.Width);
            b.Height = s.UInt8(nameof(Height), b.Height);
            b._underlay ??= new int[b.Width * b.Height];
            b._overlay ??= new int[b.Width * b.Height];

            if (s.IsReading())
                b.RawLayout = s.ByteArray("Layout", null, 3 * b.Width * b.Height);
            else
                s.ByteArray("Layout", b.RawLayout, 3 * b.Width * b.Height);

            return b;
        }
    }
}