using UAlbion.Api.Visual;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;

namespace UAlbion.Game
{
    public class DummyPaletteManager : IPaletteManager
    {
        public DummyPaletteManager(AlbionPalette palette) => Palette = palette;
        public IPalette Palette { get; }
        public IReadOnlyTexture<uint> PaletteTexture => null;
        public int Version => 0;
        public int Frame => 0;
        public float PaletteBlend => 0;
    }
}
