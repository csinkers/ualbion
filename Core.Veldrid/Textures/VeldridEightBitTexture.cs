using System;
using System.Collections.Generic;
using UAlbion.Core.Textures;
using Veldrid;

namespace UAlbion.Core.Veldrid.Textures
{
    public class VeldridEightBitTexture : EightBitTexture, IVeldridTexture
    {
        public PixelFormat Format => PixelFormat.R8_UNorm;
        public TextureType Type => TextureType.Texture2D;

        public unsafe Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage)
        {
            using (Texture staging = rf.CreateTexture(new TextureDescription(Width, Height, Depth, MipLevels, ArrayLayers, Format, TextureUsage.Staging, Type)))
            {
                staging.Name = "T_" + Name + "_Staging";

                ulong offset = 0;
                fixed (byte* texDataPtr = &TextureData[0])
                {
                    for (uint level = 0; level < MipLevels; level++)
                    {
                        uint mipWidth = GetDimension(Width, level);
                        uint mipHeight = GetDimension(Height, level);
                        uint mipDepth = GetDimension(Depth, level);
                        uint subresourceSize = mipWidth * mipHeight * mipDepth * FormatSize;

                        for (uint layer = 0; layer < ArrayLayers; layer++)
                        {
                            gd.UpdateTexture(
                                staging, (IntPtr)(texDataPtr + offset), subresourceSize,
                                0, 0, 0, mipWidth, mipHeight, mipDepth,
                                level, layer);
                            offset += subresourceSize;
                        }
                    }
                }

                Texture texture = rf.CreateTexture(new TextureDescription(Width, Height, Depth, MipLevels, ArrayLayers, Format, usage, Type));
                texture.Name = "T_" + Name;

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

        public override uint FormatSize
        {
            get
            {
                switch (Format)
                {
                    case PixelFormat.R8_G8_B8_A8_UNorm: return 4;
                    case PixelFormat.R8_UNorm: return 1;
                    case PixelFormat.R8_UInt: return 1;
                    default: throw new NotImplementedException();
                }
            }
        }

        public VeldridEightBitTexture(string name, uint width, uint height, uint mipLevels, uint arrayLayers,
            byte[] textureData, IEnumerable<SubImage> subImages)
            : base(name, width, height, mipLevels, arrayLayers, textureData, subImages)
        {
        }
    }
}