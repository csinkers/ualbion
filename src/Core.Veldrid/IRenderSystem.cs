using Veldrid;

namespace UAlbion.Core.Veldrid;

public interface IRenderSystem
{
    void Render(GraphicsDevice graphicsDevice, CommandList frameCommands, FenceHolder fence);
}