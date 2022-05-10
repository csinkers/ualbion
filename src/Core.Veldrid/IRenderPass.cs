using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public interface IRenderPass
{
    void Render(GraphicsDevice device, CommandList cl);
    IFramebufferHolder Framebuffer { get; }
    RenderPass AddRenderer(IRenderer renderer);
    RenderPass AddSource(IRenderableSource source);
}