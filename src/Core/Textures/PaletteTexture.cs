using UAlbion.Api.Visual;

namespace UAlbion.Core.Textures
{
    public class PaletteTexture : Texture<uint>
    {
        const int PaletteWidth = 256;
        const int PaletteHeight = 1;

        public PaletteTexture(IAssetId id, string name, uint[] paletteData)
            : base(id, name, PaletteWidth, PaletteHeight, 1, paletteData)
        {
        }
    }
}
