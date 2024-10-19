using UAlbion.Core.Visual;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public interface IRenderManager
{
    IRenderSystem GetSystem(string name);
    IRenderer GetRenderer(string name);
    IRenderableSource GetSource(string name);
    IFramebufferHolder GetFramebuffer(string name);
}