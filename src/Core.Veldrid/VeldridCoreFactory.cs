using System;
using UAlbion.Core.Visual;

namespace UAlbion.Core.Veldrid
{
    public class VeldridCoreFactory : ICoreFactory
    {
        // public MultiTexture CreateMultiTexture(IAssetId id, string name, IPalette palette) => new VeldridMultiTexture(id, name, palette);
        public IDisposable CreateRenderDebugGroup(IRendererContext context, string name)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return new RenderDebugGroup(((VeldridRendererContext)context).CommandList, name);
        }

        // public PaletteTexture CreatePaletteTexture(IAssetId id, string name, uint[] colours) => new VeldridPaletteTexture(id, name, colours);

        public ISceneGraph CreateSceneGraph()
            => new SceneGraph();
    }
}
