using System.Collections.Generic;

namespace UAlbion.Config
{
    public interface IGeneralConfig
    {
        string ResolvePath(string relative, IDictionary<string, string> extraPaths);
        void SetPath(string pathName, string path);
    }
}
