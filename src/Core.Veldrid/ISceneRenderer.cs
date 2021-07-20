using System;
using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    public interface ISceneRenderer
    {
        void Render(GraphicsDevice device, CommandList cl);
        IFramebufferHolder Framebuffer { get; }
        SceneRenderer AddRenderer(IRenderer renderer, params Type[] types);
        SceneRenderer AddSource(IRenderableSource source);
    }
}