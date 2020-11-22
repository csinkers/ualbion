using System.Collections.Generic;

namespace UAlbion.Config
{
    public interface IGeneralConfig
    {
        string BasePath { get; }
        IDictionary<string, string> Paths { get; }
        IList<string> SearchPaths { get; }
        string ResolvePath(string relative);
    }
}
