namespace UAlbion.Core.Veldrid;

public interface IMenuItem
{
    string Name { get; }
    string Path { get; }
    void Draw(IImGuiManager manager);
}