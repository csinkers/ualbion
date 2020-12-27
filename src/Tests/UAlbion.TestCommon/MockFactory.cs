using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;

namespace UAlbion.TestCommon
{
    public class MockFactory : ICoreFactory
    {
        public MultiTexture CreateMultiTexture(ITextureId id, string name, IPaletteManager paletteManager)
            => new MockMultiTexture(id, name, paletteManager);
        public IDisposable CreateRenderDebugGroup(IRendererContext context, string name)
            => new MockDisposable();

        public ITexture CreateEightBitTexture(
            ITextureId id,
            string name, 
            uint width, uint height,
            uint mipLevels, uint arrayLayers,
            byte[] pixels, IEnumerable<SubImage> subImages) =>
            new MockTexture(id, name, width, height, mipLevels, arrayLayers, pixels, subImages);

        public PaletteTexture CreatePaletteTexture(ITextureId id, string name, uint[] colours)
            => new MockPaletteTexture(id, name, colours);

        public ISceneGraph CreateSceneGraph()
            => new MockSceneGraph();
    }
}