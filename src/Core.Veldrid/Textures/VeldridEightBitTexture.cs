using System;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Api;
using UAlbion.Core.Textures;
using Veldrid;

namespace UAlbion.Core.Veldrid.Textures
{
    public class VeldridEightBitTexture : EightBitTexture, IVeldridTexture
    {
        public TextureType Type => TextureType.Texture2D;

        public unsafe Texture CreateDeviceTexture(GraphicsDevice gd, ResourceFactory rf, TextureUsage usage)
        {
            if (gd == null) throw new ArgumentNullException(nameof(gd));
            if (rf == null) throw new ArgumentNullException(nameof(rf));
            using Texture staging = rf.CreateTexture(new TextureDescription(Width, Height, Depth, MipLevels, ArrayLayers, Format.ToVeldrid(), TextureUsage.Staging, Type));
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

            Texture texture = rf.CreateTexture(new TextureDescription(Width, Height, Depth, MipLevels, ArrayLayers, Format.ToVeldrid(), usage, Type));
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

        public VeldridEightBitTexture(string name, uint width, uint height, uint mipLevels, uint arrayLayers,
            byte[] textureData, IEnumerable<SubImage> subImages)
            : base(name, width, height, mipLevels, arrayLayers, textureData, subImages)
        {
        }

        public Image<Rgba32> ToImage(uint[] palette)
        {
            var totalPixels = SubImages.Sum(x => (long) x.Size.X * (long) x.Size.Y);
            var width = Math.Max((int)Math.Sqrt(totalPixels), SubImages.Max(x => (int)x.Size.X));
            width = ApiUtil.NextPowerOfTwo(width);

            // First arrange to determine required size and positions, then create the image.
            var positions = new Dictionary<int, (int,int)>();
            int rowHeight = 0;
            int curX = 0, curY = 0;
            for (var index = 0; index < SubImages.Count; index++)
            {
                var si = SubImages[index];
                int w = (int) si.Size.X;
                int h = (int) si.Size.Y;

                if (width - (curX + w) >= 0) // Still room left on this row
                {
                    positions[index] = (curX, curY);
                    curX += w;
                    if (h > rowHeight)
                        rowHeight = h;
                }
                else // Start a new row
                {
                    curY += rowHeight;
                    rowHeight = h;
                    positions[index] = (0, curY);
                    curX = w;
                }
            }

            if (curX > 0)
                curY += rowHeight;

            var height = curY;

            Rgba32[] rgbaPixels = new Rgba32[width * height];
            unsafe
            {
                fixed (Rgba32* pixelPtr = rgbaPixels)
                {
                    for (var index = 0; index < SubImages.Count; index++)
                    {
                        GetSubImageOffset(index, out var siw, out var sih, out var offset, out var stride);
                        ReadOnlySpan<byte> fromSlice = TextureData.Slice(offset, siw + (sih - 1) * stride);
                        var from = new ReadOnlyByteImageBuffer((uint)siw, (uint)sih, (uint)stride, fromSlice);
                        var (toX, toY) = positions[index];
                        Span<uint> toBuffer = new Span<uint>((uint*)pixelPtr, rgbaPixels.Length);
                        toBuffer = toBuffer.Slice(toX + toY * width);
                        var to = new UIntImageBuffer((uint)siw, (uint)sih, width, toBuffer);
                        CoreUtil.Blit8To32(from, to, palette, 255, 0);
                    }
                }
            }

            Image<Rgba32> image = new Image<Rgba32>(width, height);
            image.Frames.AddFrame(rgbaPixels);
            image.Frames.RemoveFrame(0);
            return image;
        }

        public Image<Rgba32> ToImage(int subImage, uint[] palette)
        {
            GetSubImageOffset(subImage, out var width, out var height, out var offset, out var stride);

            Rgba32[] rgbaPixels = new Rgba32[width * height];
            unsafe
            {
                fixed (Rgba32* pixelPtr = rgbaPixels)
                {
                    ReadOnlySpan<byte> fromSlice = TextureData.Slice(offset, width + (height - 1) * stride);
                    var from = new ReadOnlyByteImageBuffer((uint)width, (uint)height, (uint)stride, fromSlice);

                    Span<uint> toBuffer = new Span<uint>((uint*)pixelPtr, rgbaPixels.Length);
                    var to = new UIntImageBuffer((uint)width, (uint)height, width, toBuffer);
                    CoreUtil.Blit8To32(from, to, palette, 255, 0);
                }
            }

            Image<Rgba32> image = new Image<Rgba32>(width, height);
            image.Frames.AddFrame(rgbaPixels);
            image.Frames.RemoveFrame(0);
            return image;
        }
    }
}
