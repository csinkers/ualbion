namespace UAlbion.Core.Veldrid;

public interface IImGuiMenuManager
{
    void AddMenuItem(IMenuItem item);
    void Draw(IImGuiManager manager);
}