using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual
{
    public interface IPaletteManager
    {
        IPalette Palette { get; }
        IReadOnlyTexture<uint> PaletteTexture { get; }
        int Version { get; }
        int Frame { get; }
    }
}
