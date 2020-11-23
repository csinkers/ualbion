using System.Collections.Generic;

namespace UAlbion.Config
{
    public interface IGeneralConfig
    {
        IList<string> SearchPaths { get; }
        string ResolvePath(string relative);
        void SetPath(string pathName, string path);
    }
}
