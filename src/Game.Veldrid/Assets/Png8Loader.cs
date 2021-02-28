using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SerdesNet;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Parsers;

namespace UAlbion.Game.Veldrid.Assets
{
    public class Png8Loader : Component, IAssetLoader<AlbionSprite>
    {
        Color[] GetPalette(PaletteId paletteId)
        {
            var assets = Resolve<IAssetManager>();
            var colors = new Color[256];
            var palette = assets.LoadPalette(paletteId).GetUnambiguousPalette();
            for (int i = 0; i < 256; i++)
            {
                var (r, g, b, _) = FormatUtil.UnpackColor(palette[i]);
                colors[i] = Color.FromRgb(r, g, b);
            }

            return colors;
        }

        static PngEncoder GetEncoder(Color[] colors)
        {
            var quantiser = new SixLabors.ImageSharp.Processing.Processors.Quantization.PaletteQuantizer(colors);
            return new PngEncoder
            {
                BitDepth = PngBitDepth.Bit8,
                ColorType = PngColorType.Palette,
                Quantizer = quantiser,
            };
        }

        static byte[] Write(PngEncoder encoder, Color[] palette, AlbionSprite existing, int frameNum)
        {
            var frame = existing.Frames[frameNum];
            using var image = new Image<Rgba32>(frame.Width, frame.Height);
            for (int y = 0; y < frame.Height; y++)
            {
                var from = existing.GetRowSpan(frameNum, y);
                var to = image.GetPixelRowSpan(y);
                for (int i = 0; i < from.Length; i++)
                    to[i] = palette[from[i]];
            }

            using var stream = new MemoryStream();
            encoder.Encode(image, stream);
            stream.Position = 0;
            return stream.ToArray();
        }

        static AlbionSprite Read(Color[] palette, IList<Image> images, string name)
        {
            int totalWidth = images.Max(x => x.Width);
            int totalHeight = images.Sum(x => x.Height);
            var pixels = new byte[totalWidth * totalHeight];
            var frames = new List<AlbionSpriteFrame>();
            int currentY = 0;
            bool uniform = images.All(x => x.Width == images[0].Width && x.Height == images[0].Height);
            for (int i = 0; i < images.Count; i++)
            {
                var image = images[i];
                frames.Add(new AlbionSpriteFrame(0, currentY, image.Width, image.Height));
                for (int y = 0; y < image.Height; y++)
                {
                    throw new NotImplementedException();
                }

                currentY += image.Height;
            }

            return new AlbionSprite(name, totalWidth, totalHeight, uniform, pixels, frames);
        }

        public AlbionSprite Serdes(AlbionSprite existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (s == null) throw new ArgumentNullException(nameof(s));

            var paletteId = config.Get("PaletteId", 0);
            var palette = GetPalette(new PaletteId(AssetType.Palette, paletteId));

            if (s.IsWriting())
            {
                if (existing == null)
                    throw new ArgumentNullException(nameof(existing));
                var encoder = GetEncoder(palette);
                PackedChunks.Pack(s, existing.Frames.Count, frameNum => Write(encoder, palette, existing, frameNum));
                return existing;
            }

            // Read
            var decoder = new PngDecoder();
            var configuration = new Configuration();
            var images = new List<Image>();
            try
            {
                foreach (var bytes in PackedChunks.Unpack(s))
                {
                    using var stream = new MemoryStream(bytes);
                    images.Add(decoder.Decode(configuration, stream));
                }

                return Read(palette, images, config.Name);
            }
            finally { foreach (var image in images) image.Dispose(); }
        }

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes((AlbionSprite)existing, config, mapping, s);
    }
}
