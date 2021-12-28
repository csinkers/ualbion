using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using VeldridGen;

namespace UAlbion.CodeGen.Veldrid;

static class ShaderGenerator
{
    public static void Generate(StringBuilder sb, VeldridTypeInfo shaderType, GenerationContext context)
    {
        var extension = Path.GetExtension(shaderType.Shader.Filename);
        var headerFilename = Path.GetFileNameWithoutExtension(shaderType.Shader.Filename) + ".h" + extension;

        sb.AppendLine($@"        public static (string, string) ShaderSource()
        {{
            return (""{headerFilename}"", @""// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// !!! This file was auto-generated using VeldridGen. It should not be edited by hand. !!!
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//!#version 450 // Comments with //! are just for the VS GLSL plugin
//!#extension GL_KHR_vulkan_glsl: enable
");
        ShaderEnumGenerator.EmitEnums(sb, shaderType, context);
        EmitResourceSets(sb, shaderType, context);
        EmitInputs(sb, shaderType, context);
        EmitOutputs(sb, shaderType, context);

        sb.AppendLine(@""");
        }");
    }

    static void EmitResourceSets(StringBuilder sb, VeldridTypeInfo shaderType, GenerationContext context)
    {
        var stageValue = EnumUtil.GetEnumValue((IFieldSymbol)(shaderType.Shader.ShaderType switch
        {
            ShaderType.Vertex => context.Symbols.ShaderStages.Vertex,
            ShaderType.Fragment => context.Symbols.ShaderStages.Fragment,
            ShaderType.Compute => context.Symbols.ShaderStages.Compute,
            _ => throw new ArgumentOutOfRangeException(nameof(shaderType), $"\"{shaderType}\" shaders are currently unsupported.")
        }));

        foreach (var set in shaderType.Shader.ResourceSets.OrderBy(x => x.Item1))
        {
            if (!context.Types.TryGetValue(set.Item2, out var setInfo))
                continue;

            int binding = 0;
            foreach (var resource in setInfo.Members)
            {
                if (resource.Resource == null)
                    continue;

                if ((resource.Resource.Stages & stageValue) != 0)
                    EmitResource(sb, set.Item1, binding, setInfo, resource.Resource, context);

                binding++;
            }

            sb.AppendLine();
        }
    }

    static void EmitResource(StringBuilder sb, int setNumber, int binding, VeldridTypeInfo setInfo, ResourceInfo resource, GenerationContext context)
    {
        sb.Append("layout(set = ");
        sb.Append(setNumber);
        sb.Append(", binding = ");
        sb.Append(binding);
        sb.Append(") uniform ");
        switch (resource.ResourceType)
        {
            case ResourceType.UniformBuffer:
                if (!context.Types.TryGetValue(resource.BufferType, out var bufferType))
                {
                    context.Report(
                        $"Resource {resource.Name} in set {setInfo.Symbol.ToDisplayString()} was " +
                        $"of unknown type {resource.BufferType.ToDisplayString()}. " +
                        $"The buffer type must inherit from the {context.Symbols.UniformFormat.ToDisplayString()} interface");
                    return;
                }

                sb.Append(resource.Name);
                sb.AppendLine(" {");
                EmitUniformBuffer(sb, bufferType, context);
                sb.AppendLine("};");
                break;
            case ResourceType.Texture2D:
                sb.Append("texture2D ");
                sb.Append(resource.Name);
                sb.AppendLine("; //!");
                break;
            case ResourceType.Texture2DArray:
                sb.Append("texture2DArray ");
                sb.Append(resource.Name);
                sb.AppendLine("; //!");
                break;
            case ResourceType.Sampler:
                sb.Append("sampler ");
                sb.Append(resource.Name);
                sb.AppendLine("; //!");
                break;
            default:
                context.Report($"Resource {resource.Name} in {setInfo.Symbol.ToDisplayString()} was of unhandled type {resource.ResourceType}");
                break;
        }
    }

    static void EmitUniformBuffer(StringBuilder sb, VeldridTypeInfo bufferType, GenerationContext context)
    {
        foreach (var member in bufferType.Members)
        {
            var glslType = VeldridGenUtil.GetGlslType(member.Type, context.Symbols);
            sb.Append("    ");
            sb.Append(glslType);
            sb.Append(' ');
            sb.Append(member.Uniform.Name);
            sb.AppendLine(";");
        }
    }

    static void EmitInputs(StringBuilder sb, VeldridTypeInfo shaderType, GenerationContext context)
    {
        int location = 0;
        foreach (var layout in shaderType.Shader.Inputs.OrderBy(x => x.Item1))
            location = EmitVertexLayout(sb, context, layout.Item2, location, true);
    }

    static void EmitOutputs(StringBuilder sb, VeldridTypeInfo shaderType, GenerationContext context)
    {
        int location = 0;
        foreach (var layout in shaderType.Shader.Outputs.OrderBy(x => x.Item1))
            location = EmitVertexLayout(sb, context, layout.Item2, location, false);
    }

    static string TypeForMember(VeldridMemberInfo component, GenerationContext context, bool isInput)
    {
        if (component.Vertex != null)
            return VeldridGenUtil.GetGlslType(component.Type, context.Symbols);

        if (component.ColorAttachment != null && !isInput)
        {
            try
            {
                return VeldridGenUtil.GetGlslTypeForColorAttachment(component.ColorAttachment.Format);
            }
            catch (FormatException e) { context.Report(e.Message); }
        }

        return null;
    }

    static int EmitVertexLayout(StringBuilder sb, GenerationContext context, INamedTypeSymbol layout, int location, bool isInput)
    {
        if (!context.Types.TryGetValue(layout, out var layoutInfo))
            return location;

        sb.Append("// ");
        sb.AppendLine(layout.ToDisplayString());

        foreach (var component in layoutInfo.Members)
        {
            var glslType = TypeForMember(component, context, isInput);
            if(glslType == null)
                continue;

            sb.Append("layout(location = ");
            sb.Append(location);
            sb.Append(isInput ? ") in " : ") out ");
            if (component.Vertex?.Flat == true)
                sb.Append("flat ");
            sb.Append(glslType);
            sb.Append(' ');
            sb.Append(isInput ? 'i' : 'o');

            if (component.Vertex != null)
                sb.Append(component.Vertex.Name);
            if (component.ColorAttachment != null)
                sb.Append(component.Symbol.Name);

            sb.AppendLine(";");
            location++;
        }

        sb.AppendLine();
        return location;
    }
}