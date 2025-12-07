namespace UAlbion.Core.Veldrid.Diag;

public interface IMenuItem
{
    string Name { get; }
    string Path { get; }
    void Draw(IImGuiManager manager);
}