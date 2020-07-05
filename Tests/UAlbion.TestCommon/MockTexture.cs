using System.Collections.Generic;
using System.Linq;
using UAlbion.Core.Textures;

namespace UAlbion.TestCommon
{
    public class MockTexture : ITexture
    {
        readonly List<SubImage> _subImages;
        public MockTexture(
            string name, 
            uint width, uint height,
            byte[] pixels, IEnumerable<SubImage> subImages)
        {
            Name = name;
            Width = width;
            Height = height;
            MipLevels = 0;
            ArrayLayers = 1;
            Pixels = pixels;
            _subImages = subImages.ToList();
        }

        public MockTexture(
            string name, 
            uint width, uint height,
            uint mipLevels, uint arrayLayers,
            byte[] pixels, IEnumerable<SubImage> subImages)
        {
            Name = name;
            Width = width;
            Height = height;
            MipLevels = mipLevels;
            ArrayLayers = arrayLayers;
            Pixels = pixels;
            _subImages = subImages.ToList();
        }

        public string Name { get; }
        public uint Width { get; }
        public uint Height { get; }
        public uint Depth => 1;
        public uint MipLevels { get; }
        public uint ArrayLayers { get; }
        public bool IsDirty => false;
        public int SubImageCount => _subImages.Count;
        public byte[] Pixels { get; }
        public SubImage GetSubImageDetails(int subImage) => _subImages[subImage];
        public int SizeInBytes => (int)(Pixels.Length * FormatSize);
        public uint FormatSize => 1;
    }
}