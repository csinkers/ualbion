using UAlbion.Core.Textures;

namespace UAlbion.Game.Tests
{
    public class MockPaletteTexture : PaletteTexture
    {
        public override uint FormatSize => 1;
        public MockPaletteTexture(string name, uint[] paletteData) : base(name, paletteData) { }
    }
}