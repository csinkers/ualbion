using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats;

namespace UAlbion.Game.Assets
{
    public class FontSpriteLoader<T> : Component, IAssetLoader<IReadOnlyTexture<byte>>
        where T : IAssetLoader<IReadOnlyTexture<byte>>, new()
    {
        T _loader;

        public FontSpriteLoader()
        {
            _loader = new T();
            if (_loader is IComponent component)
                AttachChild(component);
        }

        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((IReadOnlyTexture<byte>)existing, info, mapping, s);

        public IReadOnlyTexture<byte> Serdes(IReadOnlyTexture<byte> existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (s == null) throw new ArgumentNullException(nameof(s));
            return s.IsWriting() ? Write(existing, info, mapping, s) : Read(info, mapping, s);
        }

        IReadOnlyTexture<byte> Read(AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            var font = _loader.Serdes(null, info, mapping, s);
            if (font == null)
                return null;

            var frames = new List<Region>();

            // Fix up sub-images for variable size
            foreach (var frame in font.Regions)
            {
                int width = 0;
                for (int j = frame.Y; j < frame.Y + frame.Height; j++)
                    for (int i = frame.X; i < frame.X + frame.Width; i++)
                        if (i - frame.X > width && font.PixelData[j * font.Width + i] != 0)
                            width = i - frame.X;

                frames.Add(new Region(
                    frame.X, frame.Y,
                    width + 2, frame.Height,
                    font.Width, font.Height, 0));
            }

            return new SimpleTexture<byte>(font.Id, font.Id.ToString(), font.Width, font.Height, font.PixelData, frames);
        }

        IReadOnlyTexture<byte> Write(IReadOnlyTexture<byte> existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (existing == null) throw new ArgumentNullException(nameof(existing));
            int width = info.Width;
            int height = info.Height;
            if (width == 0 || height == 0)
                throw new ArgumentException("Explicit width and height must be defined when using FontSpriteLoader", nameof(info));

            var repacked = new SimpleTexture<byte>(existing.Id, existing.Name, width, height * existing.Regions.Count);

            for (int i = 0; i < existing.Regions.Count; i++)
            {
                var oldFrame = existing.GetRegionBuffer(i);
                repacked.AddRegion(0, i * height, width, height);
                BlitUtil.BlitDirect( oldFrame, repacked.GetMutableRegionBuffer(i));
            }

            var font = _loader.Serdes(repacked, info, mapping, s);
            return font == null ? null : existing;
        }
    }
}
