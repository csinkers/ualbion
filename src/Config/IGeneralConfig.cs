namespace UAlbion.Config;

public interface IGeneralConfig
{
    string ResolvePath(string relative);
    string ResolvePathAbsolute(string relative);
    string GetPath(string pathName);
    string BasePath { get; }
}