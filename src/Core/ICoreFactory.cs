using System;
using UAlbion.Core.Visual;

namespace UAlbion.Core
{
    public interface ICoreFactory
    {
        // MultiTexture CreateMultiTexture(IAssetId id, string name, IPalette palette);
        // PaletteTexture CreatePaletteTexture(IAssetId id, string name, uint[] colours);
        IDisposable CreateRenderDebugGroup(IRendererContext context, string name);
        ISceneGraph CreateSceneGraph();
    }
}
