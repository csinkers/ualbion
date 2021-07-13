using Veldrid;
namespace UAlbion.Core.Veldrid
{
    public partial class OffscreenFramebuffer
    {
        protected override Framebuffer CreateFramebuffer(global::Veldrid.GraphicsDevice device)
        {
            if (device == null) throw new System.ArgumentNullException(nameof(device));
            _depth = device.ResourceFactory.CreateTexture(new TextureDescription(
                    Width, Height, 1, 1, 1,
                    global::Veldrid.PixelFormat.R32_Float, TextureUsage.DepthStencil, TextureType.Texture2D));

            _color = device.ResourceFactory.CreateTexture(new TextureDescription(
                    Width, Height, 1, 1, 1,
                    global::Veldrid.PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.RenderTarget, TextureType.Texture2D));

            var description = new FramebufferDescription(_depth, _color);
            return device.ResourceFactory.CreateFramebuffer(ref description);
        }

        public override OutputDescription? OutputDescription
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
}
