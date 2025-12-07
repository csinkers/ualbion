namespace UAlbion.Core.Veldrid.Diag;

public interface IImGuiMenuManager
{
    void AddMenuItem(IMenuItem item);
    IImGuiWindow CreateWindow(string name, IImGuiManager manager);
    void Draw(IImGuiManager manager);
}