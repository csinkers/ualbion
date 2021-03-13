using System;
using System.Collections.Generic;
using UAlbion.Api.Visual;
using UAlbion.Core.Textures;
using UAlbion.Core.Visual;

namespace UAlbion.Core
{
    public interface ICoreFactory
    {
        MultiTexture CreateMultiTexture(ITextureId id, string name, IPaletteManager paletteManager);
        IDisposable CreateRenderDebugGroup(IRendererContext context, string name);
        ITexture CreateEightBitTexture(
            ITextureId id,
            string name,
            int width,
            int height,
            int mipLevels,
            int arrayLayers,
            byte[] pixels,
            IEnumerable<SubImage> subImages);

        PaletteTexture CreatePaletteTexture(ITextureId id, string name, uint[] colours);
        ISceneGraph CreateSceneGraph();
    }
}
