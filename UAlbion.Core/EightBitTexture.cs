using System;
using Veldrid;

namespace UAlbion.Core
{
    public class EightBitTexture : ITexture
    {
        public PixelFormat Format => PixelFormat.R8_UNorm;
        public TextureType Type => TextureType.Texture2D;
        public uint Width { get;  }
        public uint Height { get;  }
        public uint Depth => 1;
        public uint MipLevels { get;  }
        public uint ArrayLayers { get;  }
        public string Name { get; }
        public byte[] TextureData { get;  }

        public EightBitTexture(
            string name,
            uint width,
            uint height,
            uint mipLevels,
            uint arrayLayers,
            byte[] textureData)
        {
            Name = name;
            Width = width;
            Height = height;
            MipLevels = mipLevels;
            ArrayLayers = arrayLayers;
            TextureData = textureData;
        }

        public unsafe Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage)
        {
            Texture texture = rf.CreateTexture(new TextureDescription(Width, Height, Depth, MipLevels, ArrayLayers, Format, usage, Type));
            Texture staging = rf.CreateTexture(new TextureDescription(Width, Height, Depth, MipLevels, ArrayLayers, Format, TextureUsage.Staging, Type));
            texture.Name = "T_" + Name;
            staging.Name = "T_" + Name + "_Staging";

            ulong offset = 0;
            fixed (byte* texDataPtr = &TextureData[0])
            {
                for (uint level = 0; level < MipLevels; level++)
                {
                    uint mipWidth  = GetDimension(Width, level);
                    uint mipHeight = GetDimension(Height, level);
                    uint mipDepth  = GetDimension(Depth, level);
                    uint subresourceSize = mipWidth * mipHeight * mipDepth * GetFormatSize(Format);

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

            CommandList cl = rf.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(staging, texture);
            cl.End();
            gd.SubmitCommands(cl);

            return texture;
        }

        uint GetFormatSize(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R8_G8_B8_A8_UNorm: return 4;
                case PixelFormat.R8_UNorm: return 1;
                case PixelFormat.R8_UInt: return 1;
                default: throw new NotImplementedException();
            }
        }

        public static uint GetDimension(uint largestLevelDimension, uint mipLevel)
        {
            uint ret = largestLevelDimension;
            for (uint i = 0; i < mipLevel; i++)
                ret /= 2;

            return Math.Max(1, ret);
        }
    }
}