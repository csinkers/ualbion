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

namespace UAlbion.Game.Veldrid.Assets
{
    public class PngSheetLoader : Component, IAssetLoader<AlbionSprite>
    {
        static byte[] Write(IImageEncoder encoder, uint[] palette, AlbionSprite existing)
        {
            var image = ImageUtil.PackSpriteSheet(palette, existing.Frames.Count, i =>
            {
                var frame = existing.Frames[i];
                int offset = frame.X + frame.Y * existing.Width;
                int length = frame.Width + (frame.Height - 1) * existing.Width;
                ReadOnlySpan<byte> fromSlice = existing.PixelData.AsSpan(offset, length);
                return new ReadOnlyByteImageBuffer((uint)frame.Width, (uint)frame.Height, (uint)existing.Width, fromSlice);
            });
            return FormatUtil.BytesFromStream(stream => encoder.Encode(image, stream));
        }

        static AlbionSprite Read(AssetId id, uint[] palette, Image<Rgba32> image, int subItemWidth, int subItemHeight)
        {
            var pixels = new byte[image.Width * image.Height];
            var frames = new List<AlbionSpriteFrame>();
            if (!image.TryGetSinglePixelSpan(out Span<Rgba32> rgbaSpan))
                throw new InvalidOperationException("Could not retrieve single span from Image");

            var uintSpan = MemoryMarshal.Cast<Rgba32, uint>(rgbaSpan);
            var source = new ReadOnlyUIntImageBuffer((uint)image.Width, (uint)image.Height, image.Width, uintSpan);
            var dest = new ByteImageBuffer((uint)image.Width, (uint)image.Height, (uint)image.Width, pixels);
            ImageUtil.UnpackSpriteSheet(palette, subItemWidth, subItemHeight, source, dest,
                (x,y,w,h) => frames.Add(new AlbionSpriteFrame(x, y, w, h)));

            bool uniform = frames.All(x => x.Width == frames[0].Width && x.Height == frames[0].Height);
            return new AlbionSprite(id, image.Width, image.Height, uniform, pixels, frames);
        }

        public AlbionSprite Serdes(AlbionSprite existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (s == null) throw new ArgumentNullException(nameof(s));

            var paletteId = config.Get(AssetProperty.PaletteId, 0);
            var palette = Resolve<IAssetManager>()
                .LoadPalette(new PaletteId(AssetType.Palette, paletteId))
                .GetUnambiguousPalette();

            if (config.AssetId.Type == AssetType.Font)
            {
                palette[1] = 0xffffffff;
                palette[2] = 0xffcccccc;
                palette[3] = 0xffaaaaaa;
                palette[4] = 0xff777777;
                palette[5] = 0xff555555;
            }

            if (s.IsWriting())
            {
                if (existing == null)
                    throw new ArgumentNullException(nameof(existing));
                var encoder = new PngEncoder();
                var bytes = Write(encoder, palette, existing);
                s.ByteArray(null, bytes, bytes.Length);
                return existing;
            }
            else // Read
            {
                var decoder = new PngDecoder();
                var configuration = new Configuration();
                var bytes = s.ByteArray(null, null, (int) s.BytesRemaining);
                using var stream = new MemoryStream(bytes);
                using var image = decoder.Decode<Rgba32>(configuration, stream);
                return Read(config.AssetId, palette, image, config.Width, config.Height);
            }
        }

        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes((AlbionSprite)existing, config, mapping, s);
    }
}