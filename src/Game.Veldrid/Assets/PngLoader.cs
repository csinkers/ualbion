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
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Textures;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Parsers;

namespace UAlbion.Game.Veldrid.Assets
{
    public class PngLoader : Component, IAssetLoader<IEightBitImage>
    {
        static (byte[], string) Write(IImageEncoder encoder, uint[] palette, IEightBitImage existing, int frameNum)
        {
            var frame = existing.GetSubImage(frameNum);
            var buffer = new ReadOnlyByteImageBuffer(
                frame.Width,
                frame.Height,
                existing.Width,
                existing.PixelData.AsSpan(frame.PixelOffset, frame.PixelLength));

            Image<Rgba32> image = ImageUtil.BuildImageForFrame(buffer, palette);
            var bytes = FormatUtil.BytesFromStream(stream => encoder.Encode(image, stream));
            return (bytes, null);
        }

        static IEightBitImage Read(AssetId id, uint[] palette, IList<Image<Rgba32>> images)
        {
            int totalWidth = images.Max(x => x.Width);
            int totalHeight = images.Sum(x => x.Height);
            var pixels = new byte[totalWidth * totalHeight];
            var frames = new List<AlbionSpriteFrame>();
            int currentY = 0;
            var quantizeCache = new Dictionary<uint, byte>();
            for (int i = 0; i < images.Count; i++)
            {
                Image<Rgba32> image = images[i];
                if (!image.TryGetSinglePixelSpan(out Span<Rgba32> rgbaSpan))
                    throw new InvalidOperationException("Could not retrieve single span from Image");

                frames.Add(new AlbionSpriteFrame(0, currentY, image.Width, image.Height, totalWidth));
                var uintSpan = MemoryMarshal.Cast<Rgba32, uint>(rgbaSpan);
                var from = new ReadOnlyUIntImageBuffer(image.Width, image.Height, image.Width, uintSpan);
                var byteSpan = pixels.AsSpan(currentY * totalWidth, totalWidth * (image.Height - 1) + image.Width);
                var to = new ByteImageBuffer(image.Width, image.Height, totalWidth, byteSpan);
                CoreUtil.Blit32To8(from, to, palette, quantizeCache);

                currentY += image.Height;
            }

            bool uniform = frames.All(x => x.Width == frames[0].Width && x.Height == frames[0].Height);
            return new AlbionSprite(id, totalWidth, totalHeight, uniform, pixels, frames);
        }

        public IEightBitImage Serdes(IEightBitImage existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (s == null) throw new ArgumentNullException(nameof(s));

            var paletteNum = info.Get(AssetProperty.PaletteId, 0);
            var paletteId = new PaletteId(AssetType.Palette, paletteNum);
            var palette = Resolve<IAssetManager>().LoadPalette(paletteId);
            if (palette == null)
                throw new InvalidOperationException($"Could not load palette {paletteId} ({paletteNum}) for asset {info.AssetId} in file {info.File.Filename}");
            var unambiguousPalette = palette.GetUnambiguousPalette();

            if (s.IsWriting())
            {
                if (existing == null)
                    throw new ArgumentNullException(nameof(existing));
                var encoder = new PngEncoder();
                PackedChunks.Pack(s, existing.SubImageCount, frameNum => Write(encoder, unambiguousPalette, existing, frameNum));
                return existing;
            }

            // Read
            var decoder = new PngDecoder();
            var configuration = new Configuration();
            var images = new List<Image<Rgba32>>();
            try
            {
                foreach (var (bytes, _) in PackedChunks.Unpack(s))
                {
                    using var stream = new MemoryStream(bytes);
                    images.Add(decoder.Decode<Rgba32>(configuration, stream));
                }

                return Read(info.AssetId, unambiguousPalette, images);
            }
            finally { foreach (var image in images) image.Dispose(); }
        }

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((IEightBitImage)existing, info, mapping, s);
    }
}
