using System;
using UAlbion.Api;

namespace UAlbion.Core.Visual;

public interface IShaderLoader
{
    void AddShaderDirectory(string directory);
    void ClearDirectories();
    ShaderInfo Load(string path, IFileSystem disk);
    event EventHandler<EventArgs> ShadersUpdated;
}