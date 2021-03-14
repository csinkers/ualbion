using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Api.Visual;
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
            using Texture staging = rf.CreateTexture(new TextureDescription(
                (uint)Width,
                (uint)Height,
                (uint)Depth,
                (uint)MipLevels,
                (uint)ArrayLayers,
                Format.ToVeldrid(),
                TextureUsage.Staging,
                Type));

            staging.Name = "T_" + Name + "_Staging";

            ulong offset = 0;
            fixed (byte* texDataPtr = &PixelData[0])
            {
                for (uint level = 0; level < MipLevels; level++)
                {
                    uint mipWidth = GetDimension((uint)Width, level);
                    uint mipHeight = GetDimension((uint)Height, level);
                    uint mipDepth = GetDimension((uint)Depth, level);
                    uint subresourceSize = (uint)(mipWidth * mipHeight * mipDepth * FormatSize);

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

            Texture texture = rf.CreateTexture(new TextureDescription(
                (uint)Width,
                (uint)Height,
                (uint)Depth,
                (uint)MipLevels,
                (uint)ArrayLayers,
                Format.ToVeldrid(),
                usage,
                Type));

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

        public VeldridEightBitTexture(IAssetId id, string name,
            int width, int height, int mipLevels, int arrayLayers,
            byte[] textureData, IEnumerable<SubImage> subImages)
            : base(id, name, width, height, mipLevels, arrayLayers, textureData, subImages)
        {
        }

        public Image<Rgba32> ToImage(uint[] palette) => ImageUtil.PackSpriteSheet(palette, SubImages.Count, GetSubImageBuffer);

        public Image<Rgba32> ToImage(int subImage, uint[] palette)
        {
            GetSubImageOffset(subImage, out var width, out var height, out var offset, out var stride);
            var fromSlice = PixelData.AsSpan(offset, width + (height - 1) * stride);
            var from = new ReadOnlyByteImageBuffer(width, height, stride, fromSlice);
            return ImageUtil.BuildImageForFrame(from, palette);
        }
    }
}
