using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using VeldridGen;

namespace UAlbion.CodeGen.Veldrid;

static class ShaderEnumGenerator
{
    public static void EmitEnums(StringBuilder sb, VeldridTypeInfo shaderType, GenerationContext context)
    {
        var enumTypes = FindEnumTypes(shaderType, context);
        foreach (var kvp in enumTypes)
        {
            var underlying = kvp.Key.EnumUnderlyingType;
            if (underlying == null)
                continue;

            sb.Append("// ");
            sb.AppendLine(kvp.Key.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));

            var affix = EnumAffix(underlying.SpecialType);
            foreach (var value in EnumUtil.GetEnumValues(kvp.Key).OrderBy(x => x.value))
            {
                sb.Append("#define ");
                sb.Append(TweakEnumMemberName(value.field.Name, kvp.Value));
                sb.Append(" 0x");
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0:X}", value.value);
                sb.AppendLine(affix);
            }

            sb.AppendLine();
        }
    }

    static string EnumAffix(SpecialType underlyingType) =>
        underlyingType switch
        {
            SpecialType.System_Int64 => "L",
            SpecialType.System_UInt32 => "U",
            SpecialType.System_UInt64 => "UL",
            _ => ""
        };

    static Dictionary<INamedTypeSymbol, string> FindEnumTypes(VeldridTypeInfo shaderType, GenerationContext context)
    {
        var allTypeSymbols = new List<INamedTypeSymbol>();
        allTypeSymbols.AddRange(shaderType.Shader.Inputs.Select(x => x.Item2));
        allTypeSymbols.AddRange(shaderType.Shader.Outputs.Select(x => x.Item2));

        foreach (var resourceSetType in shaderType.Shader.ResourceSets.Select(x => x.Item2))
        {
            if (!context.Types.TryGetValue(resourceSetType, out var resourceSetTypeInfo))
                continue;

            allTypeSymbols.AddRange(resourceSetTypeInfo.Members
                .Where(x => x.Resource?.BufferType != null)
                .Select(resourceType => resourceType.Resource.BufferType));
        }

#pragma warning disable RS1024 // Compare symbols correctly
        var enumTypes = new Dictionary<INamedTypeSymbol, string>(SymbolEqualityComparer.Default);
#pragma warning restore RS1024 // Compare symbols correctly

        foreach (var typeSymbol in allTypeSymbols)
        {
            if (!context.Types.TryGetValue(typeSymbol, out var typeInfo))
                continue;

            foreach (var member in typeInfo.Members)
            {
                var prefix = member.Vertex?.EnumPrefix
                             ?? member.UniformMember?.EnumPrefix
                             ?? member.StructuredMember?.EnumPrefix;

                if (prefix == null)
                    continue;

                INamedTypeSymbol memberType;
                switch (member.Symbol)
                {
                    case IPropertySymbol prop: memberType = (INamedTypeSymbol)prop.Type; break;
                    case IFieldSymbol field:   memberType = (INamedTypeSymbol)field.Type; break;
                    default: continue;
                }

                if (memberType.TypeKind != TypeKind.Enum)
                    continue;

                if (enumTypes.TryGetValue(memberType, out var existingPrefix))
                {
                    if (existingPrefix != prefix)
                    {
                        context.Error($"Member {member.Symbol.Name} of {typeSymbol.Name} was declared with enum " +
                                      $"prefix \"{prefix}\", but another attribute has already declared it with the attribute \"{existingPrefix}\"");
                    }
                }
                else enumTypes[memberType] = prefix;
            }
        }

        return enumTypes;
    }

    static string TweakEnumMemberName(string name, string prefix)
    {
        var sb = new StringBuilder();
        sb.Append(prefix);
        sb.Append('_');
        bool wasCapital = true;
        foreach (var c in name)
        {
            if (char.IsUpper(c))
            {
                if (!wasCapital)
                    sb.Append('_');
                wasCapital = true;
            }
            else wasCapital = false;
            sb.Append(char.ToUpperInvariant(c));
        }
        return sb.ToString();
    }
}