using System;
using System.Collections.Generic;
using UAlbion.Api.Visual;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;

namespace UAlbion.Core
{
    public interface ICoreFactory
    {
        MultiTexture CreateMultiTexture(IAssetId id, string name, IPaletteManager paletteManager);
        IDisposable CreateRenderDebugGroup(IRendererContext context, string name);
        ITexture CreateEightBitTexture(
            IAssetId id,
            string name,
            int width,
            int height,
            int mipLevels,
            int arrayLayers,
            byte[] pixels,
            IEnumerable<SubImage> subImages);

        PaletteTexture CreatePaletteTexture(IAssetId id, string name, uint[] colours);
        ISceneGraph CreateSceneGraph();
    }
}
