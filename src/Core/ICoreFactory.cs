using System;
using System.Collections.Generic;
using UAlbion.Api;
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
            uint width,
            uint height,
            uint mipLevels,
            uint arrayLayers,
            byte[] pixels,
            IEnumerable<SubImage> subImages);

        PaletteTexture CreatePaletteTexture(ITextureId id, string name, uint[] colours);
        ISceneGraph CreateSceneGraph();
    }
}
