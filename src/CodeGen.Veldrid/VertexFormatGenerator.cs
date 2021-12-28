using System.Linq;
using System.Text;
using VeldridGen;

namespace UAlbion.CodeGen.Veldrid;

static class VertexFormatGenerator
{
    public static void Generate(StringBuilder sb, VeldridTypeInfo type)
    {
        var members = type.Members.Where(x => (x.Vertex != null)).ToList();
        if (members.Count == 0)
            return;

        sb.AppendLine(@"        public static VertexLayoutDescription GetLayout(bool input) => new(");
        bool first = true;
        foreach (var member in members)
        {
            if (!first)
                sb.AppendLine(",");
            sb.Append($@"            new VertexElementDescription((input ? ""i"" : ""o"") + ""{member.Vertex.Name}"", VertexElementSemantic.TextureCoordinate, {member.Vertex.Format})");
            first = false;
        }
        sb.AppendLine(");");
    }
    /*
public readonly partial struct Vertex2DTextured // match access specifier, name
{
public static VertexLayoutDescription Layout = new(
    new VertexElementDescription("vPosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2), // attrib.Name
    new VertexElementDescription("vTextCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));

    Types:
    float => Float1
    Vector2 => Float2
    Vector3 => Float3
    Vector4 => Float4
    uint => UInt1
    int => Int1
    // TODO: various packed formats, like half floats, 2xbyte, 4xbyte, 2xushort etc
}
    Vertex GLSL:
// {type name}
layout(location = {index}) in vec2 {attrib.Name};
layout(location = {index}) in uint {attrib.Name};

     */
}