using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Formats.Assets;

namespace UAlbion.Game
{
    public class DummyPaletteManager : IPaletteManager
    {
        public DummyPaletteManager(AlbionPalette palette) => Palette = palette;
        public IPalette Palette { get; }
        public PaletteTexture PaletteTexture { get; } = null;
        public int Version { get; } = 0;
        public int Frame { get; } = 0;
    }
}