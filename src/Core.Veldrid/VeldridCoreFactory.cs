using System;
using System.Collections.Generic;
using UAlbion.Api.Visual;
using UAlbion.Core.Textures;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid
{
    public class VeldridCoreFactory : ICoreFactory
    {
        public MultiTexture CreateMultiTexture(IAssetId id, string name, IPaletteManager paletteManager) => new VeldridMultiTexture(id, name, paletteManager);
        public IDisposable CreateRenderDebugGroup(IRendererContext context, string name)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return new RenderDebugGroup(((VeldridRendererContext)context).CommandList, name);
        }

        public PaletteTexture CreatePaletteTexture(IAssetId id, string name, uint[] colours) => new VeldridPaletteTexture(id, name, colours);

        public ISceneGraph CreateSceneGraph()
            => new SceneGraph();

        public ITexture CreateEightBitTexture(IAssetId id, string name, int width, int height, int mipLevels,
            int arrayLayers, byte[] pixels, IEnumerable<SubImage> subImages)
            => new VeldridEightBitTexture(id, name, width, height, mipLevels,
                arrayLayers, pixels, subImages);
    }
}
