using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Veldrid;

namespace UAlbion.Core.Textures
{
    public class EightBitTexture : ITexture
    {
        public struct SubImage
        {
            public SubImage(uint x, uint y, uint w, uint h, uint layer)
            {
                X = x; Y = y; W = w; H = h; Layer = layer;
            }

            public uint X { get; }
            public uint Y { get; }
            public uint W { get; }
            public uint H { get; }
            public uint Layer { get; }
        }

        public PixelFormat Format => PixelFormat.R8_UNorm;
        public TextureType Type => TextureType.Texture2D;
        public uint Width { get; }
        public uint Height { get; }
        public uint Depth => 1;
        public uint MipLevels { get; }
        public uint ArrayLayers { get; }
        public string Name { get; }
        public byte[] TextureData { get; }
        public bool IsDirty { get; private set; }
        readonly IList<SubImage> _subImages = new List<SubImage>();
        public override string ToString() => $"8BitTexture {Name} ({Width}x{Height}, {_subImages.Count} subimages)";

        public EightBitTexture(
            string name,
            uint width,
            uint height,
            uint mipLevels,
            uint arrayLayers,
            byte[] textureData,
            IEnumerable<SubImage> subImages)
        {
            Name = name;
            Width = width;
            Height = height;
            MipLevels = mipLevels;
            ArrayLayers = arrayLayers;
            TextureData = textureData;
            if(subImages != null)
                foreach(var subImage in subImages)
                    _subImages.Add(subImage);
            IsDirty = true;
        }

        public bool ContainsColors(IEnumerable<byte> colors) => TextureData.Distinct().Intersect(colors).Any();

        public void GetSubImageDetails(int id, out Vector2 size, out Vector2 texOffset, out Vector2 texSize, out uint layer)
        {
            Debug.Assert(id == 0 || id < _subImages.Count);
            if(_subImages.Count == 0)
            {
                size = Vector2.One * 16;
                texOffset = Vector2.Zero;
                texSize = Vector2.One;
                layer = 0;
            }

            var subImage = _subImages[id];
            size = new Vector2(subImage.W, subImage.H);
            texOffset = new Vector2((float)subImage.X / Width, (float)subImage.Y / Height);
            texSize = new Vector2((float)subImage.W / Width, (float)subImage.H / Height);
            layer = subImage.Layer;
        }

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

        public void UploadSubImageToStagingTexture(GraphicsDevice gd, int subImageId, Texture staging, uint layer, uint[] palette, uint x, uint y)
        {
            unsafe
            {
                fixed (byte* texDataPtr = &TextureData[0])
                {
                    if (staging.Width < Width || staging.Height < Height)
                        return;
                    // throw new InvalidOperationException($"Tried to add an oversize ({Width}, {Height}) texture to a staging texture ({staging.Width}, {staging.Height}).");

                    var subImage = _subImages[subImageId];
                    uint subresourceSize = Width * Height * Depth * GetFormatSize(Format);
                    byte* layerPtr = texDataPtr + subImage.Layer * subresourceSize;

                    uint subImageSize = subImage.W * subImage.H;
                    uint* subImageBytes = stackalloc uint[(int)subImageSize];
                    for (int j = 0; j < subImage.H; j++)
                    {
                        byte* sourceRowPtr = layerPtr + (j + subImage.Y) * Width + subImage.X;
                        for (int i = 0; i < subImage.W; i++)
                        {
                            int index = j * (int)subImage.W + i;
                            subImageBytes[index] = palette[sourceRowPtr[i]];
                        }
                    }

                    gd.UpdateTexture(
                        staging, (IntPtr)subImageBytes, subImageSize * sizeof(uint),
                        x, y, 0, subImage.W, subImage.H, 1,
                        0, layer);
                }
            }
        }
    }
}