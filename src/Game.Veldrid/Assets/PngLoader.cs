using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SerdesNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Core.Visual;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Parsers;

namespace UAlbion.Game.Veldrid.Assets
{
    public class PngLoader : Component, IAssetLoader<AlbionSprite>
    {
        static byte[] Write(IImageEncoder encoder, uint[] palette, AlbionSprite existing, int frameNum)
        {
            var frame = existing.Frames[frameNum];

            int offset = frame.X + frame.Y * existing.Width;
            int length = frame.Width + (frame.Height - 1) * existing.Width;
            var buffer = new ReadOnlyByteImageBuffer(
                (uint)frame.Width,
                (uint)frame.Height,
                (uint)existing.Width,
                existing.PixelData.AsSpan(offset, length));

            var image = ImageUtil.BuildImageForFrame(buffer, palette);
            return FormatUtil.BytesFromStream(stream => encoder.Encode(image, stream));
        }

        static AlbionSprite Read(AssetId id, uint[] palette, IList<Image<Rgba32>> images)
        {
            int totalWidth = images.Max(x => x.Width);
            int totalHeight = images.Sum(x => x.Height);
            var pixels = new byte[totalWidth * totalHeight];
            var frames = new List<AlbionSpriteFrame>();
            int currentY = 0;
            for (int i = 0; i < images.Count; i++)
            {
                var image = images[i];
                if (!image.TryGetSinglePixelSpan(out Span<Rgba32> rgbaSpan))
                    throw new InvalidOperationException("Could not retrieve single span from Image");

                frames.Add(new AlbionSpriteFrame(0, currentY, image.Width, image.Height));
                var uintSpan = MemoryMarshal.Cast<Rgba32, uint>(rgbaSpan);
                var from = new ReadOnlyUIntImageBuffer((uint)image.Width, (uint)image.Height, image.Width, uintSpan);
                var byteSpan = pixels.AsSpan(currentY * totalWidth, totalWidth * (image.Height - 1) + image.Width);
                var to = new ByteImageBuffer((uint)image.Width, (uint)image.Height, (uint)totalWidth, byteSpan);
                CoreUtil.Blit32To8(from, to, palette);

                currentY += image.Height;
            }

            bool uniform = frames.All(x => x.Width == frames[0].Width && x.Height == frames[0].Height);
            return new AlbionSprite(id, totalWidth, totalHeight, uniform, pixels, frames);
        }

        public AlbionSprite Serdes(AlbionSprite existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (s == null) throw new ArgumentNullException(nameof(s));

            var paletteId = config.Get(AssetProperty.PaletteId, 0);
            var palette = Resolve<IAssetManager>()
                .LoadPalette(new PaletteId(AssetType.Palette, paletteId))
                .GetUnambiguousPalette();

            if (s.IsWriting())
            {
                if (existing == null)
                    throw new ArgumentNullException(nameof(existing));
                var encoder = new PngEncoder();
                PackedChunks.Pack(s, existing.Frames.Count, frameNum => Write(encoder, palette, existing, frameNum));
                return existing;
            }

            // Read
            var decoder = new PngDecoder();
            var configuration = new Configuration();
            var images = new List<Image<Rgba32>>();
            try
            {
                foreach (var bytes in PackedChunks.Unpack(s))
                {
                    using var stream = new MemoryStream(bytes);
                    images.Add(decoder.Decode<Rgba32>(configuration, stream));
                }

                return Read(config.AssetId, palette, images);
            }
            finally { foreach (var image in images) image.Dispose(); }
        }

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes((AlbionSprite)existing, config, mapping, s);
    }
}
