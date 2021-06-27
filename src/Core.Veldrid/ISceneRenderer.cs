using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    public interface ISceneRenderer
    {
        void Render(GraphicsDevice device, CommandList cl);
        IFramebufferHolder Framebuffer { get; }
    }
}