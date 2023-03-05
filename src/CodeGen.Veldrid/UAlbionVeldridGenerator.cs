using System;
using System.Text;
using Microsoft.CodeAnalysis;
using VeldridGen;

namespace UAlbion.CodeGen.Veldrid;

[Generator]
public class UAlbionVeldridGenerator : VeldridGenerator
{
    protected override void GenerateResourceSet(StringBuilder sb, VeldridTypeInfo type, GenerationContext context)
    {
        if (sb == null) throw new ArgumentNullException(nameof(sb));
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (context == null) throw new ArgumentNullException(nameof(context));
        ResourceSetGenerator.Generate(sb, type, context);
    }

    protected override void GenerateVertexFormat(StringBuilder sb, VeldridTypeInfo type, GenerationContext context)
    {
        if (sb == null) throw new ArgumentNullException(nameof(sb));
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (context == null) throw new ArgumentNullException(nameof(context));
        VertexFormatGenerator.Generate(sb, type);
    }

    protected override void GenerateFramebuffer(StringBuilder sb, VeldridTypeInfo type, GenerationContext context)
    {
        if (sb == null) throw new ArgumentNullException(nameof(sb));
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (context == null) throw new ArgumentNullException(nameof(context));
        FramebufferGenerator.Generate(sb, type);
    }

    protected override void GeneratePipeline(StringBuilder sb, VeldridTypeInfo type, GenerationContext context)
    {
        if (sb == null) throw new ArgumentNullException(nameof(sb));
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (context == null) throw new ArgumentNullException(nameof(context));
        PipelineGenerator.Generate(sb, type, context);
    }

    protected override void GenerateShader(StringBuilder sb, VeldridTypeInfo type, GenerationContext context)
    {
        if (sb == null) throw new ArgumentNullException(nameof(sb));
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (context == null) throw new ArgumentNullException(nameof(context));
        ShaderGenerator.Generate(sb, type, context);
    }
}