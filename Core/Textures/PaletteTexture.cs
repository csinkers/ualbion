using System.Numerics;

namespace UAlbion.Core.Textures
{
    public abstract class PaletteTexture : ITexture
    {
        protected const uint Width = 256;
        protected const uint Height = 256;
        static readonly SubImage SubImage = new SubImage(
            Vector2.Zero,
            new Vector2(Width, Height),
            new Vector2(Width, Height),
            0);

        uint ITexture.Width => 256;
        uint ITexture.Height => 1;
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

        public SubImage GetSubImageDetails(int subImageId) => SubImage;
    }
}
