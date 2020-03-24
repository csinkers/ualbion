using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core.Textures;
using Veldrid;

namespace UAlbion.Core.Veldrid.Textures
{
    public class TrueColorTexture : IVeldridTexture
    {
        public string Name { get; }
        public PixelFormat Format => PixelFormat.R8_G8_B8_A8_UNorm;
        public TextureType Type => TextureType.Texture2D;
        public uint Width { get; }
        public uint Height { get; }
        public uint Depth => 1;
        public uint MipLevels => 1;
        public uint ArrayLayers => 1;
        public int SubImageCount => 1;
        public bool IsDirty { get; private set; }
        public int SizeInBytes => (int)(Width * Height * FormatSize);
        public uint FormatSize => sizeof(uint);
        readonly uint[] _pixelData;
        readonly SubImage _subImage;

        public TrueColorTexture(string name, uint width, uint height, uint[] palette, byte[] pixels)
        {
            Name = name;
            IsDirty = true;
            Width = width;
            Height = height;
            _pixelData = new uint[Width * Height];
            for(uint j = 0; j < Height; j++)
            {
                for(uint i = 0; i < Width; i++)
                {
                    uint index = j * Width + i;
                    byte palettePixel = pixels[index];
                    _pixelData[index] = palette[palettePixel];
                }
            }

            _subImage = new SubImage(
                    Vector2.Zero,
                 new Vector2(Width, Height),
                 new Vector2(Width, Height),
                 0);
        }

        public TrueColorTexture(string name, uint width, uint height, uint[] pixels)
        {
            Name = name;
            IsDirty = true;
            Width = width;
            Height = height;
            ApiUtil.Assert(pixels.Length == width * height);
            _pixelData = pixels;
            _subImage = new SubImage(
                    Vector2.Zero,
                 new Vector2(Width, Height),
                 new Vector2(Width, Height),
                 0);
        }

        public SubImage GetSubImageDetails(int subImageId) => _subImage;

        public unsafe Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage)
        {
            using (Texture staging = rf.CreateTexture(new TextureDescription(Width, Height, Depth, MipLevels, ArrayLayers, Format, TextureUsage.Staging, Type)))
            {
                staging.Name = "T_" + Name + "_Staging";

                ulong offset = 0;
                fixed (uint* texDataPtr = &_pixelData[0])
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

        public static uint GetDimension(uint largestLevelDimension, uint mipLevel)
        {
            uint ret = largestLevelDimension;
            for (uint i = 0; i < mipLevel; i++)
                ret /= 2;

            return Math.Max(1, ret);
        }
    }
}
