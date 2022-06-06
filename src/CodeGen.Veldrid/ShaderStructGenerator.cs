using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using VeldridGen;

namespace UAlbion.CodeGen.Veldrid;

static class ShaderStructGenerator
{
    public static void EmitStructs(StringBuilder sb, VeldridTypeInfo shaderType, GenerationContext context)
    {
        var structTypes = FindStructTypes(shaderType, context);
        foreach (var typeInfo in structTypes)
        {
            sb.Append("struct ");
            sb.AppendLine(typeInfo.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
            sb.AppendLine("{");

            var actualMembers = typeInfo.Symbol.GetMembers();

            var missing = actualMembers.Where(x => x.Kind == SymbolKind.Field && typeInfo.Members.All(m => !m.Symbol.Equals(x, SymbolEqualityComparer.Default)));
            foreach (var member in missing)
                context.Report($"Found member in IStructuredFormat struct without a StructureAttribute: {member}");

            foreach (var member in typeInfo.Members)
            {
                var glslType = VeldridGenUtil.GetGlslType(member.Type, context.Symbols);
                sb.Append("    ");
                sb.Append(glslType);
                sb.Append(' ');
                sb.Append(member.StructuredMember.Name);
                sb.AppendLine(";");
            }

            sb.AppendLine("};");
            sb.AppendLine();
        }
    }

    static List<VeldridTypeInfo> FindStructTypes(VeldridTypeInfo shaderType, GenerationContext context)
    {
        var typeSymbols = new List<VeldridTypeInfo>();
        var stageValue = shaderType.Shader.GetStageFlags(context);

        foreach (var resourceSetType in shaderType.Shader.ResourceSets.Select(x => x.Item2))
        {
            if (!context.Types.TryGetValue(resourceSetType, out var resourceSetTypeInfo))
                continue;

            foreach (var member in resourceSetTypeInfo.Members)
            {
                // If this member doesn't apply to the current shader type, ignore it
                if ((member.Resource.Stages & stageValue) == 0)
                    continue;

                var typeSymbol = member.Resource?.BufferType;
                if (typeSymbol == null)
                    continue;

                if (!context.Types.TryGetValue(typeSymbol, out var typeInfo))
                    continue;

                if ((typeInfo.Flags & TypeFlags.IsStructuredFormat) == 0)
                    continue;

                typeSymbols.Add(typeInfo);
            }
        }

        return typeSymbols;
    }
}