using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using VeldridGen;

namespace UAlbion.CodeGen.Veldrid
{
    static class PipelineGenerator
    {
        static string LayoutHelperName(INamedTypeSymbol layout) => layout.Name + "Layout";

        public static void Generate(StringBuilder sb, VeldridTypeInfo type, GenerationContext context)
        {
            // TODO: Ensure the types actually exist, ensure that they're shaders of the appropriate type etc.
            // TODO: Ensure vertex shader outputs are compatible with fragment shader inputs
            var vshader = context.Types[type.Pipeline.VertexShader];
            var fshader = context.Types[type.Pipeline.FragmentShader];
            foreach (var input in vshader.Shader.Inputs.Where(x => x.Item3 != 0))
            {
                sb.AppendLine($@"        static VertexLayoutDescription {LayoutHelperName(input.Item2)}
        {{
            get
            {{
                var layout = {input.Item2.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.Layout;
                layout.InstanceStepRate = {input.Item3};
                return layout;
            }}
        }}
");
            }

            sb.Append($@"
        public {type.Symbol.Name}() : base(""{vshader.Shader.Filename}"", ""{fshader.Shader.Filename}"",
            new[] {{ ");

            // e.g. Vertex2DTextured.Layout, SpriteInstanceDataLayout 
            bool first = true;
            foreach (var input in vshader.Shader.Inputs.OrderBy(x => x.Item1))
            {
                if (!first)
                    sb.Append(", ");

                sb.Append(input.Item3 == 0
                    ? $"{input.Item2.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.Layout"
                    : LayoutHelperName(input.Item2));
                first = false;
            }

            sb.AppendLine("},");

            sb.Append(@"            new[] { ");
            // e.g. typeof(CommonSet), typeof(SpriteArraySet) }})
            first = true;
            foreach (var set in vshader.Shader.ResourceSets.Union(fshader.Shader.ResourceSets))
            {
                if (!first)
                    sb.Append(", ");

                sb.Append($"typeof({set.Item2.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})");
                first = false;
            }
            sb.AppendLine(" })");

            sb.AppendLine(@"        { }");

        /* e.g.
        static VertexLayoutDescription SpriteInstanceDataLayout
        {
            get
            {
                var layout = SpriteInstanceData.Layout;
                layout.InstanceStepRate = 1;
                return layout;
            }
        }

        public SpritePipeline() : base("SpriteSV.vert", "SpriteSF.frag",
            new[] { Vertex2DTextured.Layout, SpriteInstanceDataLayout },
            new[] { typeof(CommonSet), typeof(SpriteArraySet) })
        {
        }
        */
        }
    }
}
