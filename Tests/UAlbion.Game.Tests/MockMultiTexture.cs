using UAlbion.Core;
using UAlbion.Core.Textures;

namespace UAlbion.Game.Tests
{
    public class MockMultiTexture : MultiTexture
    {
        public MockMultiTexture(string name, IPaletteManager paletteManager) : base(name, paletteManager)
        {
        }

        public override uint FormatSize => 1;

        public override void SavePng(int logicalId, int tick, string path)
        {
        }
    }
}