using System;
using UAlbion.Api;
using UAlbion.Core.Textures;
using Veldrid;

namespace UAlbion.Core.Veldrid.Textures
{
    public class VeldridPaletteTexture : PaletteTexture, IVeldridTexture
    {
        public TextureType Type => TextureType.Texture2D;
        public override int FormatSize => 1;

        public unsafe Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage)
        {
            if (gd == null) throw new ArgumentNullException(nameof(gd));
            if (rf == null) throw new ArgumentNullException(nameof(rf));
            using Texture staging = rf.CreateTexture(new TextureDescription(
                (uint)Width,
                (uint)Height,
                (uint)Depth,
                (uint)MipLevels,
                (uint)ArrayLayers,
                Format.ToVeldrid(),
                TextureUsage.Staging,
                Type));

            staging.Name = Name + "_Staging";

            fixed (uint* texDataPtr = &PixelData[0])
            {
                uint subresourceSize = (uint)(Width * Height * 4);
                gd.UpdateTexture(
                    staging, (IntPtr)texDataPtr, subresourceSize,
                    0, 0, 0, (uint)Width, (uint)Height, 1, 0, 0);
            }

            Texture texture = rf.CreateTexture(new TextureDescription(
                (uint)Width,
                (uint)Height,
                (uint)Depth,
                (uint)MipLevels,
                (uint)ArrayLayers,
                Format.ToVeldrid(),
                usage,
                Type));

            texture.Name = Name;
            using (CommandList cl = rf.CreateCommandList()) // TODO: Update texture without a dedicated command list to improve perf.
            {
                cl.Begin();
                cl.CopyTexture(staging, texture);
                cl.End();
                gd.SubmitCommands(cl);
            }

            IsDirty = false;
            return texture;
        }

        public VeldridPaletteTexture(ITextureId id, string name, uint[] paletteData) : base(id, name, paletteData) { }
    }
}
