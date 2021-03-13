using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Assets
{
    public class FontSpriteLoader<T> : Component, IAssetLoader<IEightBitImage>
        where T : IAssetLoader<IEightBitImage>, new()
    {
        T _loader;

        public FontSpriteLoader()
        {
            _loader = new T();
            if (_loader is IComponent component)
                AttachChild(component);
        }

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((IEightBitImage)existing, info, mapping, s);

        public IEightBitImage Serdes(IEightBitImage existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (s == null) throw new ArgumentNullException(nameof(s));
            return s.IsWriting() ? Write(existing, info, mapping, s) : Read(info, mapping, s);
        }

        IEightBitImage Read(AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            var font = _loader.Serdes(null, info, mapping, s);
            if (font == null)
                return null;

            var frames = new List<AlbionSpriteFrame>();

            // Fix up sub-images for variable size
            for (int n = 0; n < font.SubImageCount; n++)
            {
                var frame = font.GetSubImage(n);
                int width = 0;
                for (int j = frame.Y; j < frame.Y + frame.Height; j++)
                    for (int i = frame.X; i < frame.X + frame.Width; i++)
                        if (i - frame.X > width && font.PixelData[j * font.Width + i] != 0)
                            width = i - frame.X;

                frames.Add(new AlbionSpriteFrame(
                    frame.X,
                    frame.Y,
                    width + 2,
                    frame.Height,
                    font.Width));
            }

            var assetId = AssetId.FromUInt32(font.Id.ToUInt32());
            return new AlbionSprite(assetId, font.Width, font.Height, false, font.PixelData, frames);
        }

        IEightBitImage Write(IEightBitImage existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));
            var frames = new AlbionSpriteFrame[existing.SubImageCount];
            int width = info.Width;
            int height = info.Height;
            if (width == 0 || height == 0)
                throw new ArgumentException("Explicit width and height must be defined when using FontSpriteLoader", nameof(info));

            var pixelData = new byte[width * height * existing.SubImageCount];
            for (int i = 0; i < frames.Length; i++)
            {
                var oldFrame = existing.GetSubImageBuffer(i);
                var newFrame = new AlbionSpriteFrame(0, i * height, width, height, width);

                FormatUtil.Blit(
                    oldFrame.Buffer,
                    pixelData.AsSpan(newFrame.PixelOffset, newFrame.PixelLength),
                    Math.Min(oldFrame.Width, width),
                    Math.Min(oldFrame.Height, height),
                    existing.Width,
                    width);

                frames[i] = newFrame;
            }

            var uniformFrames = new AlbionSprite(
                AssetId.FromUInt32(existing.Id.ToUInt32()),
                width, height * existing.SubImageCount,
                true,
                pixelData, frames);

            var font = _loader.Serdes(uniformFrames, info, mapping, s);
            return font == null ? null : existing;
        }
    }
}