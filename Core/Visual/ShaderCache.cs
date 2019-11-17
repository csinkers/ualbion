using Veldrid;
using Veldrid.SPIRV;

namespace UAlbion.Core.Visual
{
    public interface IShaderCache
    {
        Shader[] Get(ResourceFactory factory, string vertexShader, string fragmentShader);
    }

    public class ShaderCache : IShaderCache
    {
        public Shader[] Get(ResourceFactory factory, string vertexShader, string fragmentShader)
        {
            if (!vertexShader.StartsWith("#version"))
            {
                vertexShader = @"#version 450
" + vertexShader;
            }

            if (!fragmentShader.StartsWith("#version"))
            {
                fragmentShader = @"#version 450
" + fragmentShader;
            }

            var shaders = factory.CreateFromSpirv(ShaderHelper.Vertex(vertexShader), ShaderHelper.Fragment(fragmentShader));
            return shaders;
        }
    }
}
