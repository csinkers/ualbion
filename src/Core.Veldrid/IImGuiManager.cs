using UAlbion.Core.Visual;
using Veldrid;
using VeldridGen.Interfaces;

namespace UAlbion.Core.Veldrid;

public interface IImGuiManager
{
    int GetNextWindowId();
    void AddWindow(IImGuiWindow window);
    void Draw(GraphicsDevice device, IFramebufferHolder gameFramebuffer, ICameraProvider mainCamera, GameWindow gameWindow);
}