using System;
using System.Numerics;
using UAlbion.Api.Visual;

namespace UAlbion.Core.Textures
{
    public abstract class PaletteTexture : ITexture, IRgbaImage
    {
        const int PaletteWidth = 256;
        const int PaletteHeight = 1;
        readonly uint[] _pixelData;

        static readonly SubImage SubImage = new SubImage(
            Vector2.Zero,
            new Vector2(PaletteWidth, PaletteHeight),
            new Vector2(PaletteWidth, PaletteHeight),
            0);

        public ITextureId Id { get; }
        public string Name { get; }
        public int Width => PaletteWidth;
        public int Height => PaletteHeight;
        public int Depth => 1;
        public int MipLevels => 1;
        public int ArrayLayers => 1;
        public int SubImageCount => 1;
        public bool IsDirty { get; protected set; }
        public ReadOnlySpan<uint> PixelData => _pixelData;
        public int SizeInBytes => PixelData.Length * sizeof(uint);
        public PixelFormat Format => PixelFormat.Rgba32;
        public abstract int FormatSize { get; }

        public PaletteTexture(ITextureId id, string name, uint[] paletteData)
        {
            Id = id;
            Name = name;
            _pixelData = paletteData ?? throw new ArgumentNullException(nameof(paletteData));
            IsDirty = true;
        }

        public ISubImage GetSubImage(int subImage) => SubImage;
        public void Invalidate() => IsDirty = true;
    }
}
