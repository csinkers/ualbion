using System;
using System.Collections.Generic;
using UAlbion.Api.Visual;
using UAlbion.Core;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;

namespace UAlbion.TestCommon
{
    public class MockFactory : ICoreFactory
    {
        public MultiTexture CreateMultiTexture(IAssetId id, string name, IPalette palette)
            => new MockMultiTexture(id, name, palette);
        public IDisposable CreateRenderDebugGroup(IRendererContext context, string name)
            => new MockDisposable();

        public ITexture CreateEightBitTexture(
            IAssetId id,
            string name, 
            int width, int height,
            int mipLevels, int arrayLayers,
            byte[] pixels, IEnumerable<SubImage> subImages) =>
            new MockTexture(id, name, width, height, mipLevels, arrayLayers, pixels, subImages);

        public PaletteTexture CreatePaletteTexture(IAssetId id, string name, uint[] colours)
            => new MockPaletteTexture(id, name, colours);

        public ISceneGraph CreateSceneGraph()
            => new MockSceneGraph();
    }
}