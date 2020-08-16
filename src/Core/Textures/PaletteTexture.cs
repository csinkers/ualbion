using System;
using System.Numerics;

namespace UAlbion.Core.Textures
{
    public abstract class PaletteTexture : ITexture
    {
        const uint PaletteWidth = 256;
        const uint PaletteHeight = 1;
        readonly uint[] _textureData;

        static readonly SubImage SubImage = new SubImage(
            Vector2.Zero,
            new Vector2(PaletteWidth, PaletteHeight),
            new Vector2(PaletteWidth, PaletteHeight),
            0);

        public uint Width => PaletteWidth;
        public uint Height => PaletteHeight;
        public uint Depth => 1;
        public uint MipLevels => 1;
        public uint ArrayLayers => 1;
        public int SubImageCount => 1;
        public string Name { get; }
        public bool IsDirty { get; protected set; }
        protected ReadOnlySpan<uint> TextureData => _textureData;
        public int SizeInBytes => TextureData.Length * sizeof(uint);
        public abstract uint FormatSize { get; }

        public PaletteTexture(string name, uint[] paletteData)
        {
            Name = name;
            _textureData = paletteData ?? throw new ArgumentNullException(nameof(paletteData));
            IsDirty = true;
        }

        public SubImage GetSubImageDetails(int subImageId) => SubImage;
        public void Invalidate() => IsDirty = true;
    }
}
