using Veldrid;
namespace UAlbion.Core.Veldrid.Sprites
{
    public partial struct GpuSpriteInstanceData
    {
        public static VertexLayoutDescription GetLayout(bool input) => new(
            new VertexElementDescription((input ? "i" : "o") + "Flags", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1),
            new VertexElementDescription((input ? "i" : "o") + "InstancePos", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            new VertexElementDescription((input ? "i" : "o") + "Size", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription((input ? "i" : "o") + "TexOffset", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription((input ? "i" : "o") + "TexSize", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription((input ? "i" : "o") + "TexLayer", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1));
    }
}
