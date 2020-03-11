using System;
using System.Collections.Generic;
using UAlbion.Core.Textures;

namespace UAlbion.Core
{
    public interface ICoreFactory
    {
        MultiTexture CreateMultiTexture(string name, IPaletteManager paletteManager);
        IDisposable CreateRenderDebugGroup(IRendererContext context, string name);
        ITexture CreateEightBitTexture(
            string name,
            uint width,
            uint height,
            uint mipLevels,
            uint arrayLayers,
            byte[] pixels,
            IEnumerable<SubImage> subImages);

        PaletteTexture CreatePaletteTexture(string name, uint[] colours);
    }
}