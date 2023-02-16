using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public interface IRenderPass<in TGlobalSet>
    where TGlobalSet : IResourceSetHolder
{
    string Name { get; }
    IFramebufferHolder Framebuffer { get; }
    void Render(GraphicsDevice device, CommandList cl, TGlobalSet globalSet);
}