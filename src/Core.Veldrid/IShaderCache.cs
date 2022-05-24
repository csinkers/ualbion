using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid;

public interface IShaderCache
{
    Shader[] GetShaderPair(ResourceFactory factory, ShaderInfo vertex, ShaderInfo fragment);
    void CleanupOldFiles();
}