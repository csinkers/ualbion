using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class HeaderBasedSpriteLoader : IAssetLoader<AlbionSprite>
    {
        public object Serdes(object existing, AssetInfo config, AssetMapping mapping, ISerializer s)
            => Serdes((AlbionSprite)existing, config, mapping, s);

        public AlbionSprite Serdes(AlbionSprite existing, AssetInfo config, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (s.IsWriting() && existing == null) throw new ArgumentNullException(nameof(existing));

            ApiUtil.Assert(config.Transposed != true);

            int width = s.UInt16("Width", (ushort?)existing?.Frames[0].Width ?? 0);
            int height = s.UInt16("Height", (ushort?)existing?.Frames[0].Height ?? 0);
            int something = s.UInt8(null, 0);
            ApiUtil.Assert(something == 0);
            byte frameCount = s.UInt8("Frames", (byte?)existing?.Frames.Count ?? 1);

            bool uniform = config.File?.Format != "NonUniform";
            // TODO: When writing, assert that uniform and the frame sizes match up
            var frames = existing?.Frames.ToArray() ?? new AlbionSpriteFrame[frameCount];
            var allFrames = new List<byte[]>(frameCount * width * height);
            int currentY = 0;

            int spriteWidth = 0;
            for (int i = 0; i < frameCount; i++)
            {
                var frame = frames[i];
                if (!uniform && i > 0)
                {
                    width = s.UInt16("FrameWidth", (ushort?)frame?.Width ?? 0);
                    height = s.UInt16("FrameHeight", (ushort?)frame?.Height ?? 0);
                    something = s.UInt8(null, 0);
                    ApiUtil.Assert(something == 0);
                    byte spriteCount2 = s.UInt8(null, frameCount);
                    ApiUtil.Assert(spriteCount2 == frameCount);
                }

                byte[] frameBytes = null;
                if (s.IsWriting())
                {
                    frameBytes = new byte[width * height];
                    Debug.Assert(existing != null, nameof(existing) + " != null");
                    Debug.Assert(frame != null, nameof(frame) + " != null");

                    FormatUtil.Blit(
                        existing.PixelData.AsSpan(frame.Y * existing.Width + frame.X),
                        frameBytes.AsSpan(),
                        frame.Width, frame.Height,
                        existing.Width, frame.Width);
                }
                frameBytes = s.ByteArray("Frame" + i, frameBytes, width * height);;
                frames[i] ??= new AlbionSpriteFrame(0, currentY, width, height);
                allFrames.Add(frameBytes);

                currentY += height;
                if (width > spriteWidth)
                    spriteWidth = width;
            }

            if (existing != null)
            {
                ApiUtil.Assert(spriteWidth == existing.Width,
                    "HeaderSprite: Expected calculated and existing sprite width to be equal");
                return existing;
            }

            byte[] pixelData = new byte[spriteWidth * currentY];
            for (int n = 0; n < frameCount; n++)
            {
                var frame = frames[n];
                FormatUtil.Blit(
                    allFrames[n],
                    pixelData.AsSpan(frame.Y*spriteWidth + frame.X),
                    frame.Width, frame.Height,
                    frame.Width, spriteWidth);
            }

            s.Check();
            return new AlbionSprite(config.AssetId.ToString(), spriteWidth, currentY, uniform, pixelData, frames);
        }
    }
}
