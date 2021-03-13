using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Config;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.Parsers
{
    public class MultiHeaderSpriteLoader : IAssetLoader<IEightBitImage>
    {
        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s)
            => Serdes((IEightBitImage)existing, info, mapping, s);

        public IEightBitImage Serdes(IEightBitImage existing, AssetInfo info, AssetMapping mapping, ISerializer s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (s.IsWriting() && existing == null) throw new ArgumentNullException(nameof(existing));

            int width = s.UInt16("Width", (ushort?)existing?.GetSubImage(0).Width ?? 0);
            int height = s.UInt16("Height", (ushort?)existing?.GetSubImage(0).Height ?? 0);
            int something = s.UInt8(null, 0);
            ApiUtil.Assert(something == 0);
            byte frameCount = s.UInt8("Frames", (byte?)existing?.SubImageCount ?? 1);

            // TODO: When writing, assert that uniform and the frame sizes match up
            var frames = new (int, int, int)[frameCount];

            if (existing != null)
            {
                for (int i = 0; i < existing.SubImageCount; i++)
                {
                    var x = existing.GetSubImage(i);
                    frames[i] = (x.Y, x.Width, x.Height);
                }
            }

            var allFrames = new List<byte[]>(frameCount * width * height);
            int currentY = 0;

            int totalWidth = 0;
            for (int i = 0; i < frameCount; i++)
            {
                var (fy, fw, fh) = frames[i];
                if (i > 0)
                {
                    width = s.UInt16("FrameWidth", (ushort)fw);
                    height = s.UInt16("FrameHeight", (ushort)fh);
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

                    FormatUtil.Blit(
                        existing.PixelData.AsSpan(fy * existing.Width),
                        frameBytes.AsSpan(),
                        fw, fh,
                        existing.Width, fw);
                }
                frameBytes = s.Bytes("Frame" + i, frameBytes, width * height);
                frames[i] = (currentY, width, height);
                allFrames.Add(frameBytes);

                currentY += height;
                if (width > totalWidth)
                    totalWidth = width;
            }

            if (existing != null)
            {
                ApiUtil.Assert(totalWidth == existing.Width,
                    "MultiHeaderSprite: Expected calculated and existing sprite width to be equal");
                return existing;
            }

            byte[] pixelData = new byte[totalWidth * currentY];
            for (int n = 0; n < frameCount; n++)
            {
                var (fy, fw, fh) = frames[n];
                FormatUtil.Blit(
                    allFrames[n],
                    pixelData.AsSpan(fy * totalWidth),
                    fw, fh,
                    fw, totalWidth);
            }

            s.Check();
            return new AlbionSprite(
                info.AssetId,
                totalWidth,
                currentY,
                false,
                pixelData,
                frames.Select(frame =>
                {
                    var (y, w, h) = frame;
                    return new AlbionSpriteFrame(0, y, w, h, totalWidth);
                }));
        }
    }
}
