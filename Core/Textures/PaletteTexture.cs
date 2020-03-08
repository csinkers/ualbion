using System.Numerics;

namespace UAlbion.Core.Textures
{
    public abstract class PaletteTexture : ITexture
    {
        public uint Width => 256;
        public uint Height => 1;
        public uint Depth => 1;
        public uint MipLevels => 1;
        public uint ArrayLayers => 1;
        public int SubImageCount => 1;
        public string Name { get; }
        public bool IsDirty { get; protected set; }
        protected uint[] TextureData { get;  }
        public int SizeInBytes => TextureData.Length * sizeof(uint);
        public abstract uint FormatSize { get; }

        public PaletteTexture(string name, uint[] paletteData)
        {
            Name = name;
            TextureData = paletteData;
            IsDirty = true;
        }

        public void GetSubImageDetails(int subImage, out Vector2 size, out Vector2 texOffset, out Vector2 texSize, out uint layer)
        {
            size = Vector2.One;
            texOffset = Vector2.Zero;
            texSize = Vector2.One;
            layer = 0;
        }
    }
}