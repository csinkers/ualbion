using System;
using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Core.Textures;

namespace UAlbion.TestCommon
{
    public class MockFactory : ICoreFactory
    {
        public MultiTexture CreateMultiTexture(string name, IPaletteManager paletteManager) 
            => new MockMultiTexture(name, paletteManager);
        public IDisposable CreateRenderDebugGroup(IRendererContext context, string name) 
            => new MockDisposable();

        public ITexture CreateEightBitTexture(
            string name, 
            uint width, uint height,
            uint mipLevels, uint arrayLayers,
            byte[] pixels, IEnumerable<SubImage> subImages) =>
            new MockTexture(name, width, height, mipLevels, arrayLayers, pixels, subImages);

        public PaletteTexture CreatePaletteTexture(string name, uint[] colours)
            => new MockPaletteTexture(name, colours);

        public ISceneGraph CreateSceneGraph()
            => new MockSceneGraph();
    }
}