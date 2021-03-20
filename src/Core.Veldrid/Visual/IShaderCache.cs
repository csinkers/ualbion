using UAlbion.Core.Visual;
using Veldrid;

namespace UAlbion.Core.Veldrid.Visual
{
    public interface IVeldridShaderCache : IShaderCache
    {
        Shader[] GetShaderPair(ResourceFactory factory,
            string vertexShaderName, string fragmentShaderName,
            string vertexShaderContent, string fragmentShaderContent);
    }
}
