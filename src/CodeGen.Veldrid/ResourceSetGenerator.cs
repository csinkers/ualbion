using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using VeldridGen;

namespace UAlbion.CodeGen.Veldrid;

static class ResourceSetGenerator
{
    public static void Generate(StringBuilder sb, VeldridTypeInfo type, GenerationContext context)
    {
        /* e.g.
        new ResourceLayoutElementDescription("uSprite", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
        new ResourceLayoutElementDescription("uSpriteSampler", ResourceKind.Sampler, ShaderStages.Fragment),
        new ResourceLayoutElementDescription("_Uniform", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment));
        */
        sb.AppendLine("        public static readonly ResourceLayoutDescription Layout = new(");
        bool first = true;
        foreach (var member in type.Members.Where(x => x.Resource != null))
        {
            if (!first)
                sb.AppendLine(",");

            var shaderStages = member.Resource.Stages.ToString(CultureInfo.InvariantCulture); // Util.FormatFlagsEnum(member.Resource.Stages);
            var kindString = context.Symbols.Veldrid.ResourceKind.KnownKindString(member.Resource.Kind);
            var resourceName =
                member.Resource.Kind is KnownResourceKind.StructuredBufferReadOnly or KnownResourceKind.StructuredBufferReadWrite
                    ? member.Resource.Name + "Buffer"
                    : member.Resource.Name;

            sb.Append($"            new ResourceLayoutElementDescription(\"{resourceName}\", global::{kindString}, (ShaderStages){shaderStages})");
            first = false;
        }
        sb.AppendLine(");");
        sb.AppendLine();

        foreach (var member in type.Members.Where(x => x.Resource != null))
        {
            switch (member.Resource.Kind)
            {
                case KnownResourceKind.UniformBuffer:
                case KnownResourceKind.StructuredBufferReadOnly:
                case KnownResourceKind.StructuredBufferReadWrite:
                    GenerateBuffer(sb, member, context);
                    break;

                case KnownResourceKind.TextureReadOnly:
                case KnownResourceKind.TextureReadWrite:
                    GenerateTexture(sb, member, context);
                    break;

                case KnownResourceKind.Sampler:
                    GenerateSampler(sb, member, context);
                    break;

                default:
                    context.Report($"Resource {member.Symbol.ToDisplayString()} was of unexpected kind {member.Resource.Kind}");
                    break;
            }
        }

        /* e.g. protected override ResourceSet Build(GraphicsDevice device, ResourceLayout layout) =>
            device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                layout,
                _globalInfo.DeviceBuffer,
                _projection.DeviceBuffer,
                _view.DeviceBuffer,
                _palette.DeviceTexture)); */

        sb.AppendLine(@"        protected override ResourceSet Build(GraphicsDevice device, ResourceLayout layout)
        {
#if DEBUG");

        foreach (var member in type.Members.Where(x => x.Resource != null))
        {
            if (member.Symbol is not IFieldSymbol field)
            {
                context.Report($"Resource set backing members must be fields (member {member.Symbol.ToDisplayString()} in {type.Symbol.ToDisplayString()} was a {member.Symbol.GetType().Name})");
                continue;
            }

            sb.Append("                if (");
            sb.Append(field.Name);
            sb.Append('.');
            AppendDeviceMemberForKind(sb, member, context);

            sb.AppendFormat(CultureInfo.InvariantCulture,
                " == null) throw new System.InvalidOperationException(\"Tried to construct {0}, but {1} has not been initialised. It may not have been attached to the exchange.\");{2}",
                type.Symbol.Name,
                VeldridGenUtil.UnderscoreToTitleCase(field.Name),
                Environment.NewLine);
        }

        sb.AppendLine(@"#endif
");

        sb.Append(@"            return device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                layout");

        foreach (var member in type.Members.Where(x => x.Resource != null))
        {
            sb.AppendLine(",");
            var field = (IFieldSymbol)member.Symbol;
            sb.Append("                ");
            sb.Append(field.Name);
            sb.Append('.');
            AppendDeviceMemberForKind(sb, member, context);
        }
 
        sb.AppendLine("));");
        sb.AppendLine("        }");

        sb.AppendLine(@"
        protected override void Resubscribe()
        {");
        foreach (var member in type.Members.Where(x => x.Resource != null))
        {
            if (member.Type.AllInterfaces.Any(x => x.Equals(context.Symbols.BuiltIn.NotifyPropertyChanged, SymbolEqualityComparer.Default)))
            {
                var field = (IFieldSymbol)member.Symbol;
                sb.AppendLine(
                    $@"            if ({field.Name} != null)
                {field.Name}.PropertyChanged += PropertyDirty;");
            }
        }

        sb.AppendLine("        }");

        sb.AppendLine(@"
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);");
        foreach (var member in type.Members.Where(x => x.Resource != null))
        {
            if (member.Type.AllInterfaces.Any(x => x.Equals(context.Symbols.BuiltIn.NotifyPropertyChanged, SymbolEqualityComparer.Default)))
            {
                var field = (IFieldSymbol)member.Symbol;
                sb.AppendLine(
                    $@"            if ({field.Name} != null)
                {field.Name}.PropertyChanged -= PropertyDirty;");
            }
        }

        sb.AppendLine("        }");
    }

    static void AppendDeviceMemberForKind(StringBuilder sb, VeldridMemberInfo member, GenerationContext context)
    {
        switch (member.Resource.Kind)
        {
            case KnownResourceKind.UniformBuffer:
            case KnownResourceKind.StructuredBufferReadOnly:
            case KnownResourceKind.StructuredBufferReadWrite:
                sb.Append("DeviceBuffer");
                break;

            case KnownResourceKind.TextureReadOnly:
            case KnownResourceKind.TextureReadWrite:
                sb.Append("DeviceTexture");
                break;

            case KnownResourceKind.Sampler:
                sb.Append("Sampler");
                break;

            case KnownResourceKind.Unknown:
            default:
                context.Report($"Resource {member.Symbol.ToDisplayString()} was of unexpected kind \"{member.Resource.Kind}\"");
                break;
        }
    }

    static void GenerateSampler(StringBuilder sb, VeldridMemberInfo member, GenerationContext context)
    {
        if (member.Symbol is not IFieldSymbol field)
        {
            context.Report($"Resource set backing members must be fields (member {member.Symbol.ToDisplayString()} in " +
                           $"{member.Symbol.ContainingType.ToDisplayString()} was a {member.Symbol.GetType().Name})");
            return;
        }

        /* e.g.
    public SamplerHolder Sampler
    {
        get => _sampler;
        set
        {
            if (_sampler == value) return; 
            if (_sampler != null) _sampler.PropertyChanged -= PropertyDirty; 
            _sampler = value; 
            if (_sampler != null) _sampler.PropertyChanged += PropertyDirty;
            Dirty();
        }
    } */
        var propertyName = VeldridGenUtil.UnderscoreToTitleCase(field.Name);
        sb.AppendLine($@"        public {field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {propertyName}
        {{
            get => {field.Name};
            set
            {{
                if ({field.Name} == value) 
                    return;

                if ({field.Name} != null)
                    {field.Name}.PropertyChanged -= PropertyDirty;

                {field.Name} = value;

                if ({field.Name} != null)
                    {field.Name}.PropertyChanged += PropertyDirty;
                Dirty();
            }}
        }}
");
    }

    static void GenerateTexture(StringBuilder sb, VeldridMemberInfo member, GenerationContext context)
    {
        if (member.Symbol is not IFieldSymbol field)
        {
            context.Report($"Resource set backing members must be fields (member {member.Symbol.ToDisplayString()} in " +
                           $"{member.Symbol.ContainingType.ToDisplayString()} was a {member.Symbol.GetType().Name})");
            return;
        }

        /* e.g.
    public Texture2DHolder Palette
    {
        get => _palette;
        set
        {
            if (_palette == value) return;
            if (_palette != null) _palette.PropertyChanged -= PropertyDirty;
            _palette = value;
            if (_palette != null) _palette.PropertyChanged += PropertyDirty;
            Dirty();
        }
    } */
        var propertyName = VeldridGenUtil.UnderscoreToTitleCase(field.Name);
        sb.AppendLine($@"        public {field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {propertyName}
        {{
            get => {field.Name};
            set
            {{
                if ({field.Name} == value) return;

                if ({field.Name} != null)
                    {field.Name}.PropertyChanged -= PropertyDirty;

                {field.Name} = value;

                if ({field.Name} != null)
                    {field.Name}.PropertyChanged += PropertyDirty;
                Dirty();
            }}
        }}
");
    }

    static void GenerateBuffer(StringBuilder sb, VeldridMemberInfo member, GenerationContext context)
    {
        if (member.Symbol is not IFieldSymbol field)
        {
            context.Report($"Resource set backing members must be fields (member {member.Symbol.ToDisplayString()} in " +
                           $"{member.Symbol.ContainingType.ToDisplayString()} was a {member.Symbol.GetType().Name})");
            return;
        }

        /* e.g.
        public SingleBuffer<GlobalInfo> GlobalInfo
        {
            get => _globalInfo;
            set
            {
                if (_globalInfo == value)
                    return;
                _globalInfo = value;
                Dirty();
            }
        }*/
        var propertyName = VeldridGenUtil.UnderscoreToTitleCase(field.Name);
        sb.AppendLine($@"        public {field.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {propertyName}
        {{
            get => {field.Name};
            set
            {{
                if ({field.Name} == value)
                    return;
                {field.Name} = value;
                Dirty();
            }}
        }}
");
    }
}