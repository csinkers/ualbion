using System.Collections.Generic;
using UAlbion.Core.Veldrid.Sprites;

namespace UAlbion.Core.Veldrid
{
    public static class ShaderHeaders
    {
        public static IEnumerable<(string, string)> All => new[]
        {
            SpriteVertexShader.ShaderSource(),
            SpriteFragmentShader.ShaderSource(),
            EtmVertexShader.ShaderSource(),
            EtmFragmentShader.ShaderSource(),
            SkyboxVertexShader.ShaderSource(),
            SkyboxFragmentShader.ShaderSource(),
        };
    }
}
