using UAlbion.Core.Textures;

namespace UAlbion.TestCommon
{
    public class MockPaletteTexture : PaletteTexture
    {
        public override uint FormatSize => 1;
        public MockPaletteTexture(string name, uint[] paletteData) : base(name, paletteData) { }
    }
}