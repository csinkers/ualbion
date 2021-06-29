using Veldrid;
namespace UAlbion.Core.Veldrid.Sprites
{
    public partial struct GpuSpriteInstanceData
    {
        public static VertexLayoutDescription Layout = new(
            new VertexElementDescription("InstancePos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription("Size", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("TexOffset", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("TexSize", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("TexLayer", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1),
            new VertexElementDescription("Flags", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1));
    }
}
