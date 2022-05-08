using Veldrid;
namespace UAlbion.Core.Veldrid.Sprites
{
    internal partial struct BlendedSpriteIntermediateData
    {
        public static VertexLayoutDescription GetLayout(bool input) => new(
            new VertexElementDescription((input ? "i" : "o") + "Flags", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1),
            new VertexElementDescription((input ? "i" : "o") + "TexPosition1", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription((input ? "i" : "o") + "Layer1", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
            new VertexElementDescription((input ? "i" : "o") + "TexPosition2", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription((input ? "i" : "o") + "Layer2", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
            new VertexElementDescription((input ? "i" : "o") + "NormCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription((input ? "i" : "o") + "WorldPosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));
    }
}
