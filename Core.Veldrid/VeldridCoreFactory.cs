using System;
using System.Collections.Generic;
using UAlbion.Core.Textures;
using UAlbion.Core.Veldrid.Textures;

namespace UAlbion.Core.Veldrid
{
    public class VeldridCoreFactory : ICoreFactory
    {
        public MultiTexture CreateMultiTexture(string name, IPaletteManager paletteManager) => new VeldridMultiTexture(name, paletteManager);
        public IDisposable CreateRenderDebugGroup(IRendererContext context, string name)
            => new RenderDebugGroup(((VeldridRendererContext)context).CommandList, name);

        public PaletteTexture CreatePaletteTexture(string name, uint[] colours) => new VeldridPaletteTexture(name, colours);

        public ISceneGraph CreateSceneGraph()
            => new SceneGraph();

        public ITexture CreateEightBitTexture(string name, uint width, uint height, uint mipLevels,
            uint arrayLayers, byte[] pixels, IEnumerable<SubImage> subImages)
            => new VeldridEightBitTexture(name, width, height, mipLevels,
                arrayLayers, pixels, subImages);
    }
}
