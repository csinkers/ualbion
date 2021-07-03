using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid
{
    public interface IRenderer : IComponent
    {
        void Render(IRenderable renderable, CommonSet commonSet, IFramebufferHolder framebuffer, CommandList cl,
            GraphicsDevice device);
    }
}