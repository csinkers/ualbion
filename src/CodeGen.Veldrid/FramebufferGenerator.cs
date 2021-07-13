using System.Linq;
using System.Text;
using VeldridGen;

namespace UAlbion.CodeGen.Veldrid
{
    static class FramebufferGenerator
    {
        public static void Generate(StringBuilder sb, VeldridTypeInfo type)
        {
            // TODO: Decouple from UAlbion.Core etc, make more flexible
            var depth = type.Members.SingleOrDefault(x => x.DepthAttachment != null);
            sb.AppendLine(@"        protected override Framebuffer CreateFramebuffer(global::Veldrid.GraphicsDevice device)
        {
            if (device == null) throw new System.ArgumentNullException(nameof(device));");

            if (depth != null)
            {
                sb.AppendLine($@"            {depth.Symbol.Name} = device.ResourceFactory.CreateTexture(new TextureDescription(
                    Width, Height, 1, 1, 1,
                    global::{depth.DepthAttachment.Format}, TextureUsage.DepthStencil, TextureType.Texture2D));
");
            }

            foreach (var color in type.Members.Where(member => member.ColorAttachment != null))
            {
                sb.AppendLine($@"            {color.Symbol.Name} = device.ResourceFactory.CreateTexture(new TextureDescription(
                    Width, Height, 1, 1, 1,
                    global::{color.ColorAttachment.Format}, TextureUsage.RenderTarget, TextureType.Texture2D));
");
            }

            sb.Append("            var description = new FramebufferDescription(");
            sb.Append(depth != null ? depth.Symbol.Name : "null");

            foreach (var member in type.Members.Where(member => member.ColorAttachment != null))
            {
                sb.Append(", ");
                sb.Append(member.Symbol.Name);
            }

            sb.AppendLine($@");
            return device.ResourceFactory.CreateFramebuffer(ref description);
        }}

        public override OutputDescription? OutputDescription
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

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);");
            if (depth != null)
            {
                sb.AppendLine($@"            {depth.Symbol.Name}?.Dispose();");
                sb.AppendLine($@"            {depth.Symbol.Name} = null;");
            }

            foreach (var member in type.Members.Where(member => member.ColorAttachment != null))
            {
                sb.AppendLine($@"            {member.Symbol.Name}?.Dispose();");
                sb.AppendLine($@"            {member.Symbol.Name} = null;");
            }

            sb.AppendLine(@"        }");
        }
        /* e.g.
        public partial class OffscreenFramebuffer
        {
            protected override Framebuffer CreateFramebuffer(GraphicsDevice device)
            {
                _depth = device.ResourceFactory.CreateTexture(new TextureDescription(
                    Width, Height, 1, 1, 1,
                    PixelFormat.R32_Float, TextureUsage.DepthStencil, TextureType.Texture2D));

                _color = device.ResourceFactory.CreateTexture(new TextureDescription(
                    Width, Height, 1, 1, 1,
                    PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.RenderTarget, TextureType.Texture2D));

                var description = new FramebufferDescription(_depth, _color);
                return device.ResourceFactory.CreateFramebuffer(ref description);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                _depth?.Dispose();
                _color?.Dispose();
                _depth = null;
                _color = null;
            }
        } */
    }
}
