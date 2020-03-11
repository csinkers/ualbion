using System;
using UAlbion.Core.Textures;
using Veldrid;

namespace UAlbion.Core.Veldrid.Textures
{
    public class VeldridPaletteTexture : PaletteTexture, IVeldridTexture
    {
        public PixelFormat Format => PixelFormat.R8_G8_B8_A8_UNorm;
        public TextureType Type => TextureType.Texture2D;
        public override uint FormatSize => 1;

        public unsafe Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage)
        {
            using (Texture staging = rf.CreateTexture(new TextureDescription(Width, Height, Depth, MipLevels,
                ArrayLayers, Format, TextureUsage.Staging, Type)))
            {
                staging.Name = Name + "_Staging";

                fixed (uint* texDataPtr = &TextureData[0])
                {
                    uint subresourceSize = Width * Height * 4;
                    gd.UpdateTexture(
                        staging, (IntPtr)texDataPtr, subresourceSize,
                        0, 0, 0, Width, Height, 1, 0, 0);
                }

                Texture texture = rf.CreateTexture(new TextureDescription(Width, Height, Depth, MipLevels, ArrayLayers, Format, usage, Type));
                texture.Name = Name;
                using (CommandList cl = rf.CreateCommandList())
                {
                    cl.Begin();
                    cl.CopyTexture(staging, texture);
                    cl.End();
                    gd.SubmitCommands(cl);
                }

                IsDirty = false;
                return texture;
            }
        }

        public VeldridPaletteTexture(string name, uint[] paletteData) : base(name, paletteData)
        {
        }
    }
}
