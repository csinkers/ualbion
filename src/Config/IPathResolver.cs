namespace UAlbion.Config;

public interface IPathResolver
{
    string BasePath { get; }
    string ResolvePath(string relative);
    string ResolvePathAbsolute(string relative);
    void RegisterPath(string name, string path);
    string GetPath(string pathName);
}