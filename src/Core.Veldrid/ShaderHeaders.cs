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
            Etm.EtmVertexShader.ShaderSource(),
            Etm.EtmFragmentShader.ShaderSource(),
            FullscreenQuadVertexShader.ShaderSource(),
            FullscreenQuadFragmentShader.ShaderSource(),
            Skybox.SkyboxVertexShader.ShaderSource(),
            Skybox.SkyboxFragmentShader.ShaderSource(),
        };
    }
}
