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

            var shaderStages = member.Resource.Stages.ToString(); // Util.FormatFlagsEnum(member.Resource.Stages);
            sb.Append($"            new ResourceLayoutElementDescription(\"{member.Resource.Name}\", global::{member.Resource.Kind}, (ShaderStages){shaderStages})");
            first = false;
        }
        sb.AppendLine(");");
        sb.AppendLine();

        foreach (var member in type.Members.Where(x => x.Resource != null))
        {
            if (Equals(member.Resource.Kind, context.Symbols.ResourceKind.UniformBuffer.ToDisplayString())) GenerateUniform(sb, member, context);
            else if (Equals(member.Resource.Kind, context.Symbols.ResourceKind.TextureReadOnly.ToDisplayString())) GenerateTexture(sb, member, context);
            else if (Equals(member.Resource.Kind, context.Symbols.ResourceKind.Sampler.ToDisplayString())) GenerateSampler(sb, member, context);
            else context.Report($"Resource {member.Symbol.ToDisplayString()} in set {type.Symbol.ToDisplayString()} was of unexpected kind {member.Resource.Kind}");
        }

        /* e.g. protected override ResourceSet Build(GraphicsDevice device, ResourceLayout layout) =>
            device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                layout,
                _globalInfo.DeviceBuffer,
                _projection.DeviceBuffer,
                _view.DeviceBuffer,
                _palette.DeviceTexture)); */

        sb.Append(@"        protected override ResourceSet Build(GraphicsDevice device, ResourceLayout layout) =>
            device.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                layout");

        foreach (var member in type.Members.Where(x => x.Resource != null))
        {
            sb.AppendLine(",");

            if (member.Symbol is not IFieldSymbol field)
            {
                context.Report($"Resource set backing members must be fields (member {member.Symbol.ToDisplayString()} in {type.Symbol.ToDisplayString()} was a {member.Symbol.GetType().Name})");
                continue;
            }

            sb.Append("                ");
            sb.Append(field.Name);
            sb.Append('.');

            if (Equals(member.Resource.Kind, context.Symbols.ResourceKind.UniformBuffer.ToDisplayString())) sb.Append("DeviceBuffer");
            else if (Equals(member.Resource.Kind, context.Symbols.ResourceKind.TextureReadOnly.ToDisplayString())) sb.Append("DeviceTexture");
            else if (Equals(member.Resource.Kind, context.Symbols.ResourceKind.Sampler.ToDisplayString())) sb.Append("Sampler");
            else context.Report($"Resource {member.Symbol.ToDisplayString()} in {type.Symbol.ToDisplayString()} was of unexpected kind \"{member.Resource.Kind}\"");
        }
 
        sb.AppendLine("));");

        sb.AppendLine(@"
        protected override void Resubscribe()
        {");
        foreach (var member in type.Members.Where(x => x.Resource != null))
        {
            if (member.Type.AllInterfaces.Any(x => x.Equals(context.Symbols.NotifyPropertyChanged, SymbolEqualityComparer.Default)))
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
            if (member.Type.AllInterfaces.Any(x => x.Equals(context.Symbols.NotifyPropertyChanged, SymbolEqualityComparer.Default)))
            {
                var field = (IFieldSymbol)member.Symbol;
                sb.AppendLine(
                    $@"            if ({field.Name} != null)
                {field.Name}.PropertyChanged -= PropertyDirty;");
            }
        }

        sb.AppendLine("        }");
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

    static void GenerateUniform(StringBuilder sb, VeldridMemberInfo member, GenerationContext context)
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