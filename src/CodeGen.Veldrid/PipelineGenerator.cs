using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using VeldridGen;

namespace UAlbion.CodeGen.Veldrid;

static class PipelineGenerator
{
    static string LayoutHelperName(INamedTypeSymbol layout) => layout.Name + "Layout";

    public static void Generate(StringBuilder sb, VeldridTypeInfo type, GenerationContext context)
    {
        // TODO: Ensure the types actually exist, ensure that they're shaders of the appropriate type etc.
        // TODO: Ensure vertex shader outputs are compatible with fragment shader inputs
        var vshader = context.Types[type.Pipeline.VertexShader];
        var fshader = context.Types[type.Pipeline.FragmentShader];
        foreach (var input in vshader.Shader.Inputs.Where(x => x.instanceStep != 0))
        {
            sb.AppendLine($@"        static VertexLayoutDescription {LayoutHelperName(input.type)}
        {{
            get
            {{
                var layout = {input.type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.GetLayout(true);
                layout.InstanceStepRate = {input.instanceStep};
                return layout;
            }}
        }}
");
        }

        sb.Append($@"        public {type.Symbol.Name}() : base(""{vshader.Shader.Filename}"", ""{fshader.Shader.Filename}"",
            new[] {{");

        // e.g. Vertex2DTextured.Layout, SpriteInstanceDataLayout 
        bool first = true;
        foreach (var input in vshader.Shader.Inputs.OrderBy(x => x.slot))
        {
            if (!first)
                sb.Append(", ");

            sb.AppendLine();
            sb.Append("                ");
            sb.Append(input.instanceStep == 0
                ? $"{input.type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}.GetLayout(true)"
                : LayoutHelperName(input.type));
            first = false;
        }

        sb.AppendLine();
        sb.AppendLine("            },");
        sb.Append("            new[] {");
        // e.g. typeof(CommonSet), typeof(SpriteArraySet) }})

        var sets = new List<INamedTypeSymbol>();
        foreach (var set in vshader.Shader.ResourceSets.Union(fshader.Shader.ResourceSets))
        {
            while (sets.Count <= set.slot)
                sets.Add(null);

            if (SymbolEqualityComparer.Default.Equals(sets[set.slot], set.type))
                continue;

            if (sets[set.slot] == null)
                sets[set.slot] = set.type;
            else
            {
                context.Report(DiagnosticSeverity.Error,
                    $"Tried to define ResourceSlot {set.slot} as {set.type}, " +
                    $"but it is already defined as {sets[set.slot]}!");
            }
        }

        for(int i = 0; i < sets.Count; i++)
            if (sets[i] == null)
                context.Report(DiagnosticSeverity.Error, $"No resource layout is defined for slot {i}!");

        first = true;
        foreach (var set in sets)
        {
            if (!first)
                sb.Append(", ");

            sb.AppendLine();
            sb.Append("                typeof(");
            sb.Append(set.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            sb.Append(')');
            first = false;
        }
        sb.AppendLine();
        sb.AppendLine("            })");
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