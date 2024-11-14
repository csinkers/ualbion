using System.Collections.Generic;
using UAlbion.Core.Veldrid.Meshes;
using UAlbion.Core.Veldrid.Sprites;

namespace UAlbion.Core.Veldrid;

public static class ShaderHeaders
{
    public static IEnumerable<(string, string)> All =>
    [
        BlendedSpriteVertexShader.ShaderSource(),
        BlendedSpriteFragmentShader.ShaderSource(),
        Etm.EtmVertexShader.ShaderSource(),
        Etm.EtmFragmentShader.ShaderSource(),
        FullscreenQuadVertexShader.ShaderSource(),
        FullscreenQuadFragmentShader.ShaderSource(),
        MeshVertexShader.ShaderSource(),
        MeshFragmentShader.ShaderSource(),
        Skybox.SkyboxVertexShader.ShaderSource(),
        Skybox.SkyboxFragmentShader.ShaderSource(),
        SpriteVertexShader.ShaderSource(),
        SpriteFragmentShader.ShaderSource(),
        TileVertexShader.ShaderSource(),
        TileFragmentShader.ShaderSource()
    ];
}