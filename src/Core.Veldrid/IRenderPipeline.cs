using Veldrid;

namespace UAlbion.Core.Veldrid;

public interface IRenderPipeline
{
    void Render(GraphicsDevice graphicsDevice);
}