using UAlbion.Api.Eventing;
using Veldrid;

namespace UAlbion.Core.Veldrid;

public interface IRenderSystem : IComponent
{
    void Render(GraphicsDevice graphicsDevice);
}