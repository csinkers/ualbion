using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;

namespace UAlbion.Core.Textures
{
    public abstract class EightBitTexture : ITexture
    {
        public static float OffsetX = 0;
        public static float OffsetY = 0;
        public static float ScaleAdjustX = 0;
        public static float ScaleAdjustY = 0;
        public struct SubImage
        {
            public SubImage(uint x, uint y, uint w, uint h, uint layer)
            {
                X = x; Y = y; W = w; H = h; Layer = layer;
            }

            public override string ToString() => $"({X}, {Y}, {W}, {H}, L{Layer})";

            public uint X { get; }
            public uint Y { get; }
            public uint W { get; }
            public uint H { get; }
            public uint Layer { get; }
        }

        public abstract uint FormatSize { get; }
        public uint Width { get; }
        public uint Height { get; }
        public uint Depth => 1;
        public uint MipLevels { get; }
        public uint ArrayLayers { get; }
        public string Name { get; }
        public byte[] TextureData { get; }
        public int SubImageCount => _subImages.Count;
        public bool IsDirty { get; protected set; }
        public IReadOnlyList<SubImage> SubImages => new ReadOnlyCollection<SubImage>(_subImages);
        public int SizeInBytes => TextureData.Length;
        readonly IList<SubImage> _subImages = new List<SubImage>();
        public override string ToString() => $"8Bit {Name} ({Width}x{Height}, {_subImages.Count} subimages)";

        public EightBitTexture(
            string name,
            uint width,
            uint height,
            uint mipLevels,
            uint arrayLayers,
            byte[] textureData,
            IEnumerable<SubImage> subImages)
        {
            Name = name;
            Width = width;
            Height = height;
            MipLevels = mipLevels;
            ArrayLayers = arrayLayers;
            TextureData = textureData;
            if(subImages != null)
                foreach(var subImage in subImages)
                    _subImages.Add(subImage);
            IsDirty = true;
        }

        public bool ContainsColors(IEnumerable<byte> colors) => TextureData.Distinct().Intersect(colors).Any();

        public void GetSubImageOffset(int id, out int width, out int height, out int offset, out int stride)
        {
            if (_subImages.Count == 0)
            {
                width = 0; height = 0; offset = 0; stride = 0;
                return;
            }

            if (id >= _subImages.Count)
                id %= _subImages.Count;

            var subImage = _subImages[id];
            uint subresourceSize = Width * Height * Depth * FormatSize;
            width = (int)subImage.W;
            height = (int)subImage.H;
            offset = (int)(subImage.Layer * subresourceSize + subImage.Y * Width + subImage.X);
            stride = (int)Width;
        }

        public void GetSubImageDetails(int id, out Vector2 size, out Vector2 texOffset, out Vector2 texSize, out uint layer)
        {
            if(_subImages.Count == 0)
            {
                size = Vector2.One * 16;
                texOffset = Vector2.Zero;
                texSize = Vector2.One;
                layer = 0;
            }

            if (id < 0)
                id = _subImages.Count + id;
            if (id >= _subImages.Count)
                id %= _subImages.Count;

            var subImage = _subImages[id];
            size = new Vector2(subImage.W, subImage.H);
            texOffset = new Vector2((subImage.X + OffsetX) / Width, (subImage.Y + OffsetY) / Height);
            texSize = new Vector2((subImage.W + ScaleAdjustX) / Width, (subImage.H + ScaleAdjustY) / Height);
            layer = subImage.Layer;
        }

        public static uint GetDimension(uint largestLevelDimension, uint mipLevel)
        {
            uint ret = largestLevelDimension;
            for (uint i = 0; i < mipLevel; i++)
                ret /= 2;

            return Math.Max(1, ret);
        }
    }
}