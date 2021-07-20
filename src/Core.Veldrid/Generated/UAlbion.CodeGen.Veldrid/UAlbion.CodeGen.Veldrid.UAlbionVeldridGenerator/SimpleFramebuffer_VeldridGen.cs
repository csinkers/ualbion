using Veldrid;
namespace UAlbion.Core.Veldrid
{
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
                    global::Veldrid.PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.RenderTarget | TextureUsage.Sampled, TextureType.Texture2D));
            Color.DeviceTexture.Name = Color.Name;

            var description = new FramebufferDescription(Depth.DeviceTexture, Color.DeviceTexture);
            var framebuffer = device.ResourceFactory.CreateFramebuffer(ref description);
            framebuffer.Name = Name;
            return framebuffer;
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
            Depth.DeviceTexture?.Dispose();
            Depth.DeviceTexture = null;
            Color.DeviceTexture?.Dispose();
            Color.DeviceTexture = null;
        }
    }
}
