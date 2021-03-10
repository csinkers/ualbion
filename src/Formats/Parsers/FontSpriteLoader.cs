using System;
using System.Collections.Generic;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class FontSpriteLoader : IAssetLoader<IEightBitImage>
    {
        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((IEightBitImage)existing, info, mapping, s);

        public IEightBitImage Serdes(IEightBitImage existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (s == null) throw new ArgumentNullException(nameof(s));

            AlbionSprite2 uniformFrames = null;
            if (s.IsWriting())
            {
                if (existing == null) throw new ArgumentNullException(nameof(existing));
                var frames = new AlbionSpriteFrame[existing.SubImageCount];
                for (int i = 0; i < frames.Length; i++)
                {
                    var frame = existing.GetSubImage(i);
                    frames[i] = new AlbionSpriteFrame(frame.X, frame.Y, info.Width, info.Height, existing.Width);
                }

                uniformFrames = new AlbionSprite2(
                    AssetId.FromUInt32(existing.Id.ToUInt32()),
                    existing.Width, existing.Height,
                    true,
                    existing.PixelData.ToArray(), frames);
            }

            var font = new FixedSizeSpriteLoader().Serdes(uniformFrames, info, mapping, s);
            if (font == null)
                return null;

            if (s.IsWriting())
            {
                return existing;
            }
            else
            {
                var frames = new List<AlbionSpriteFrame>();

                // Fix up sub-images for variable size
                for (int n = 0; n < font.SubImageCount; n++)
                {
                    var frame = font.GetSubImage(n);
                    int width = 0;
                    for (int j = frame.Y; j < frame.Y + frame.Height; j++)
                    {
                        for (int i = frame.X; i < frame.X + frame.Width; i++)
                        {
                            if (i - frame.X > width && font.PixelData[j * font.Width + i] != 0)
                                width = i - frame.X;
                        }
                    }

                    frames.Add(new AlbionSpriteFrame(frame.X, frame.Y, width + 2, frame.Height, font.Width));
                }

                var assetId = AssetId.FromUInt32(font.Id.ToUInt32());
                return new AlbionSprite2(assetId, font.Width, font.Height, false, font.PixelData.ToArray(), frames);
            }
        }
    }
}
