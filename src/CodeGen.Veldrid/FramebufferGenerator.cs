using System.Linq;
using System.Text;
using VeldridGen;
using SymbolDisplayFormat = Microsoft.CodeAnalysis.SymbolDisplayFormat;

namespace UAlbion.CodeGen.Veldrid;

static class FramebufferGenerator
{
    const string TexturesNamespace = "global::UAlbion.Core.Veldrid.Textures";
    public static void Generate(StringBuilder sb, VeldridTypeInfo type)
    {
        var depth = type.Members.SingleOrDefault(x => x.DepthAttachment != null);
        BuildConstructor(sb, type, depth);
        BuildCreateFramebuffer(sb, type, depth);
        BuildOutputDescription(sb, type, depth);
        BuildDispose(sb, type, depth);
    }

    static void BuildConstructor(StringBuilder sb, VeldridTypeInfo type, VeldridMemberInfo depth)
    {
        var typeName = type.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        sb.AppendLine(
            $@"        public {typeName}(string name, uint width, uint height) : base(name, width, height)
        {{");
        if (depth != null)
            sb.AppendLine($@"            {depth.Symbol.Name} = new {TexturesNamespace}.Texture2DHolder(name + "".{depth.Symbol.Name}"");");

        foreach (var color in type.Members.Where(member => member.ColorAttachment != null))
            sb.AppendLine($@"            {color.Symbol.Name} = new {TexturesNamespace}.Texture2DHolder(name + "".{color.Symbol.Name}"");");

        sb.AppendLine(@"        }
");
    }

    static void BuildCreateFramebuffer(StringBuilder sb, VeldridTypeInfo type, VeldridMemberInfo depth)
    {
        sb.AppendLine(@"        protected override Framebuffer CreateFramebuffer(global::Veldrid.GraphicsDevice device)
        {
            System.ArgumentNullException.ThrowIfNull(device);");

        if (depth != null)
        {
            sb.AppendLine($@"            {depth.Symbol.Name}.DeviceTexture = device.ResourceFactory.CreateTexture(new TextureDescription(
                    Width, Height, 1, 1, 1,
                    global::{depth.DepthAttachment.Format}, TextureUsage.DepthStencil, TextureType.Texture2D));
            {depth.Symbol.Name}.DeviceTexture.Name = {depth.Symbol.Name}.Name;
");
        }

        foreach (var color in type.Members.Where(member => member.ColorAttachment != null))
        {
            sb.AppendLine($@"            {color.Symbol.Name}.DeviceTexture = device.ResourceFactory.CreateTexture(new TextureDescription(
                    Width, Height, 1, 1, 1,
                    global::{color.ColorAttachment.Format}, TextureUsage.RenderTarget | TextureUsage.Sampled, TextureType.Texture2D));
            {color.Symbol.Name}.DeviceTexture.Name = {color.Symbol.Name}.Name;
");
        }

        sb.Append("            var description = new FramebufferDescription(");
        sb.Append(depth != null ? depth.Symbol.Name + ".DeviceTexture" : "null");

        foreach (var member in type.Members.Where(member => member.ColorAttachment != null))
        {
            sb.Append(", ");
            sb.Append(member.Symbol.Name);
            sb.Append(".DeviceTexture");
        }

        sb.AppendLine(@");
            var framebuffer = device.ResourceFactory.CreateFramebuffer(in description);
            framebuffer.Name = Name;
            return framebuffer;
        }
");
    }

    static void BuildOutputDescription(StringBuilder sb, VeldridTypeInfo type, VeldridMemberInfo depth)
    {
        sb.AppendLine($@"        public static OutputDescription Output
        {{
            get
            {{
                OutputAttachmentDescription? depthAttachment = {(depth == null ? "null" : $"new(global::{depth.DepthAttachment.Format})")};
                OutputAttachmentDescription[] colorAttachments =
                {{");
        bool first = true;
        foreach (var color in type.Members.Where(x => x.ColorAttachment != null))
        {
            if (!first)
                sb.AppendLine(",");
            sb.Append($"                    new(global::{color.ColorAttachment.Format})");
            first = false;
        }

        sb.AppendLine(@"
                };
                return new OutputDescription(depthAttachment, colorAttachments);
            }
        }

        public override OutputDescription? OutputDescription => Output;
");

    }

    static void BuildDispose(StringBuilder sb, VeldridTypeInfo type, VeldridMemberInfo depth)
    {
        sb.AppendLine(@"        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);");
        if (depth != null)
        {
            sb.AppendLine($@"            {depth.Symbol.Name}.DeviceTexture?.Dispose();");
            sb.AppendLine($@"            {depth.Symbol.Name}.DeviceTexture = null;");
        }

        foreach (var member in type.Members.Where(member => member.ColorAttachment != null))
        {
            sb.AppendLine($@"            {member.Symbol.Name}.DeviceTexture?.Dispose();");
            sb.AppendLine($@"            {member.Symbol.Name}.DeviceTexture = null;");
        }

        sb.AppendLine(@"        }");
    }

    /* e.g.
public partial class SimpleFramebuffer
{
    public SimpleFramebuffer(uint width, uint height, string name) : base(width, height, name)
    {
        Depth = new global::UAlbion.Core.Veldrid.Textures.Texture2DHolder(name + ".Depth");
        Color = new global::UAlbion.Core.Veldrid.Textures.Texture2DHolder(name + ".Color");
    }

    protected override Framebuffer CreateFramebuffer(global::Veldrid.GraphicsDevice device)
    {
        if (device == null) throw new System.ArgumentNullException(nameof(device));
        Depth.DeviceTexture = device.ResourceFactory.CreateTexture(new TextureDescription(
                Width, Height, 1, 1, 1,
                global::Veldrid.PixelFormat.R32_Float, TextureUsage.DepthStencil, TextureType.Texture2D));
        Depth.DeviceTexture.Name = Depth.Name;

        Color.DeviceTexture = device.ResourceFactory.CreateTexture(new TextureDescription(
                Width, Height, 1, 1, 1,
                global::Veldrid.PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.RenderTarget, TextureType.Texture2D));
        Color.DeviceTexture.Name = Color.Name;

        var description = new FramebufferDescription(_depth, _color);
        var framebuffer = device.ResourceFactory.CreateFramebuffer(in description);
        framebuffer.Name = Name;
        return framebuffer;
    }

    public global::UAlbion.Core.Veldrid.Textures.Texture2DHolder Depth { get; }
    public global::UAlbion.Core.Veldrid.Textures.Texture2DHolder Color { get; }

    public override OutputDescription? OutputDescription => Output;
    public static OutputDescription OutputDescription
    {
        get
        {
            OutputAttachmentDescription? depthAttachment = new(global::Veldrid.PixelFormat.R32_Float);
            OutputAttachmentDescription[] colorAttachments =
            {
                new(global::Veldrid.PixelFormat.B8_G8_R8_A8_UNorm)
            };
            return new OutputDescription(depthAttachment, colorAttachments);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _depth?.Dispose();
        _depth = null;
        _color?.Dispose();
        _color = null;
    }
}
     */
}