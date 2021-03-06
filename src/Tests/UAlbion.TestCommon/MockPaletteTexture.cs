using UAlbion.Api;
using UAlbion.Core.Textures;

namespace UAlbion.TestCommon
{
    public class MockPaletteTexture : PaletteTexture
    {
        public override int FormatSize => 1;
        public MockPaletteTexture(ITextureId id, string name, uint[] paletteData) : base(id, name, paletteData) { }
    }
}