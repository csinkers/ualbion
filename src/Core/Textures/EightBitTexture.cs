using System;
using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Core.Textures
{
    public abstract class EightBitTexture : ITexture
    {
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
        public IReadOnlyList<SubImage> SubImages => _subImages.AsReadOnly();
        public int SizeInBytes => TextureData.Length;
        readonly List<SubImage> _subImages = new List<SubImage>();
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
            width = (int)subImage.Size.X;
            height = (int)subImage.Size.Y;
            offset = (int)(subImage.Layer * subresourceSize + subImage.Offset.Y * Width + subImage.Offset.X);
            stride = (int)Width;
        }

        public SubImage GetSubImageDetails(int id)
        {
            if(_subImages.Count == 0)
                return null;

            if (id < 0)
                id = _subImages.Count + id;

            if (id >= _subImages.Count)
                id %= _subImages.Count;

            return _subImages[id];
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
