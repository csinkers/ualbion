using UAlbion.Api;
using UAlbion.Core.Textures;

namespace UAlbion.Core
{
    public interface IPaletteManager
    {
        IPalette Palette { get; }
        PaletteTexture PaletteTexture { get; }
        int Version { get; }
        int Frame { get; }
    }
}