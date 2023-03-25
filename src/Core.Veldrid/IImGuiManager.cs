namespace UAlbion.Core.Veldrid;

public interface IImGuiManager
{
    int GetNextWindowId();
    void AddWindow(IImGuiWindow window);
    void Draw();
}