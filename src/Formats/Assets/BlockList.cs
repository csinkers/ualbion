using System;
using System.Collections.Generic;
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

        public static IList<Block> Serdes(int _, IList<Block> blockList, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            blockList ??= new List<Block>();
            if (s.Mode == SerializerMode.Reading)
            {
                int j = 0;
                while (!s.IsComplete())
                {
                    blockList.Add(SerdesBlock(j, null, s));
                    j++;
                }
            }
            else
            {
                s.List(null, blockList, blockList.Count, SerdesBlock);
            }

            return blockList;
        }

        static Block SerdesBlock(int _, Block b, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            b ??= new Block();
            b.Width = s.UInt8(nameof(Width), b.Width);
            b.Height = s.UInt8(nameof(Height), b.Height);
            b._underlay ??= new int[b.Width * b.Height];
            b._overlay ??= new int[b.Width * b.Height];

            if (s.Mode == SerializerMode.Reading)
                b.RawLayout = s.ByteArray("Layout", null, 3 * b.Width * b.Height);
            else
                s.ByteArray("Layout", b.RawLayout, 3 * b.Width * b.Height);
            /*
            for (int i = 0; i < b._underlay.Length; i++)
            {
                var underlay = b._underlay[i];
                var overlay = b._overlay[i];

                byte b1 = (byte)((overlay & 0xff0) >> 4);
                byte b2 = (byte) (((overlay & 0xf) << 4) | ((underlay & 0xf00) >> 8));
                byte b3 = (byte)(underlay & 0xff);

                b1 = s.UInt8("0", b1);
                b2 = s.UInt8("1", b2);
                b3 = s.UInt8("2", b3);

                b._underlay[i] = ((b2 & 0x0F) << 8) + b3;
                b._overlay[i] = (b1 << 4) + (b2 >> 4);
            } */

            return b;
        }
    }
} 