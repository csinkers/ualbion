using UAlbion.Api.Eventing;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public interface IRenderPass : IContainer
{
    void Render(GraphicsDevice device, CommandList cl);
    IFramebufferHolder Framebuffer { get; }
}