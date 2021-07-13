﻿using System;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Api.Visual;
using Veldrid;
using Veldrid.ImageSharp;

namespace UAlbion.Core.Veldrid.Textures
{
    public static class VeldridTexture
    {
        static uint MipLevelCount(int width, int height)
        {
            int maxDimension = Math.Max(width, height);
            int levels = 1;
            while (maxDimension > 1)
            {
                maxDimension >>= 1;
                levels++;
            }
            return (uint)levels;
        }

        static PixelFormat GetFormat(Type pixelType)
        {
            if (pixelType == typeof(byte)) return PixelFormat.R8_UNorm;
            if (pixelType == typeof(uint)) return PixelFormat.R8_G8_B8_A8_UNorm;
            throw new NotSupportedException();
        }

        public static unsafe Texture CreateSimpleTexture<T>(GraphicsDevice gd, TextureUsage usage, IReadOnlyTexture<T> texture) where T : unmanaged
        {
            if (gd == null) throw new ArgumentNullException(nameof(gd));
            if (texture == null) throw new ArgumentNullException(nameof(texture));

            var pixelFormat = GetFormat(typeof(T));
            bool mip = (usage & TextureUsage.GenerateMipmaps) != 0;
            uint mipLevels = mip ? MipLevelCount(texture.Width, texture.Height) : 1;
            using Texture staging = gd.ResourceFactory.CreateTexture(new TextureDescription(
                (uint)texture.Width, (uint)texture.Height, 1,
                mipLevels,
                1,
                pixelFormat,
                TextureUsage.Staging,
                TextureType.Texture2D));

            staging.Name = "T_" + texture.Name + "_Staging";

            var buffer = texture.PixelData;
            fixed (T* texDataPtr = &buffer[0])
            {
                gd.UpdateTexture(
                    staging, (IntPtr)texDataPtr, (uint)(buffer.Length * Unsafe.SizeOf<T>()),
                    0, 0, 0,
                    (uint)texture.Width, (uint)texture.Height, 1,
                    0, 0);
            }

            Texture veldridTexture = gd.ResourceFactory.CreateTexture(new TextureDescription(
                (uint)texture.Width, (uint)texture.Height, 1,
                mipLevels,
                1,
                pixelFormat,
                usage,
                TextureType.Texture2D));

            veldridTexture.Name = "T_" + texture.Name;

            using CommandList cl = gd.ResourceFactory.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(staging, veldridTexture);
            if (mip) cl.GenerateMipmaps(veldridTexture);
            cl.End();
            gd.SubmitCommands(cl);

            return veldridTexture;
        }

        public static unsafe Texture CreateArrayTexture<T>(GraphicsDevice gd, TextureUsage usage, IReadOnlyTexture<T> texture) where T : unmanaged
        {
            if (gd == null) throw new ArgumentNullException(nameof(gd));
            if (texture == null) throw new ArgumentNullException(nameof(texture));

            var pixelFormat = GetFormat(typeof(T));
            bool mip = (usage & TextureUsage.GenerateMipmaps) != 0;
            uint mipLevels = mip ? MipLevelCount(texture.Width, texture.Height) : 1;
            using Texture staging = gd.ResourceFactory.CreateTexture(new TextureDescription(
                (uint)texture.Width, (uint)texture.Height, 1,
                mipLevels,
                (uint)texture.ArrayLayers,
                pixelFormat,
                TextureUsage.Staging,
                TextureType.Texture2D));

            staging.Name = "T_" + texture.Name + "_Staging";

            for (int layer = 0; layer < texture.ArrayLayers; layer++)
            {
                var buffer = texture.GetLayerBuffer(layer);
                fixed (T* texDataPtr = &buffer.Buffer[0])
                {
                    gd.UpdateTexture(
                        staging, (IntPtr)texDataPtr, (uint)(buffer.Buffer.Length * Unsafe.SizeOf<T>()),
                        0, 0, 0,
                        (uint)texture.Width, (uint)texture.Height, 1,
                        0, (uint)layer);
                }
            }

            Texture veldridTexture = gd.ResourceFactory.CreateTexture(new TextureDescription(
                (uint)texture.Width, (uint)texture.Height, 1,
                mipLevels,
                (uint)texture.ArrayLayers,
                pixelFormat,
                usage,
                TextureType.Texture2D));

            veldridTexture.Name = "T_" + texture.Name;

            using CommandList cl = gd.ResourceFactory.CreateCommandList();
            cl.Begin();
            cl.CopyTexture(staging, veldridTexture);
            if (mip) cl.GenerateMipmaps(veldridTexture);
            cl.End();
            gd.SubmitCommands(cl);

            return veldridTexture;
        }
        public static Texture CreateSimpleTexture(GraphicsDevice gd, TextureUsage usage, SixLabors.ImageSharp.Image<Rgba32> image, string name)
        {
            if (gd == null) throw new ArgumentNullException(nameof(gd));
            if (usage != TextureUsage.Sampled && usage != (TextureUsage.Sampled | TextureUsage.GenerateMipmaps))
                throw new ArgumentOutOfRangeException(nameof(usage), "Only sampled texture usage is currently supported (with optional mipmapping)");

            ImageSharpTexture imageSharpTexture = new ImageSharpTexture(image, (usage & TextureUsage.GenerateMipmaps) != 0);
            var texture = imageSharpTexture.CreateDeviceTexture(gd, gd.ResourceFactory);
            texture.Name = "T_" + name;
            return texture;
        }
    }
}