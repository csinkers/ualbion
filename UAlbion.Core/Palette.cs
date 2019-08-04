using System;
using Veldrid;

namespace UAlbion.Core
{
    public class Palette : ITexture
    {
        public PixelFormat Format => PixelFormat.R8_G8_B8_A8_UNorm;
        public TextureType Type => TextureType.Texture2D;
        public uint Width => 256;
        public uint Height => 1;
        public uint Depth => 1;
        public uint MipLevels => 1;
        public uint ArrayLayers => 1;
        public string Name { get; }
        uint[] TextureData { get;  }

        public Palette(string name, uint[] paletteData)
        {
            Name = name;
            TextureData = paletteData;
        }

        public unsafe Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage)
        {
            Texture texture = rf.CreateTexture(new TextureDescription(Width, Height, Depth, MipLevels, ArrayLayers, Format, usage, Type));
            Texture staging = rf.CreateTexture(new TextureDescription(Width, Height, Depth, MipLevels, ArrayLayers, Format, TextureUsage.Staging, Type));
            texture.Name = Name;
            staging.Name = Name + "_Staging";

            fixed (uint* texDataPtr = &TextureData[0])
            {
                uint subresourceSize = Width * Height * 4;
                gd.UpdateTexture(
                    staging, (IntPtr)texDataPtr, subresourceSize,
                    0, 0, 0, Width, Height, 1, 0, 0);
            }

            CommandList cl = rf.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(staging, texture);
            cl.End();
            gd.SubmitCommands(cl);

            return texture;
        }
    }
}