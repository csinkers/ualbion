using System;
using System.Linq;
using SerdesNet;
using UAlbion.Api;
using UAlbion.Api.Visual;
using UAlbion.Config;

namespace UAlbion.Formats.Parsers
{
    public class SingleHeaderSpriteLoader : IAssetLoader<IReadOnlyTexture<byte>>
    {
        public object Serdes(object existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
            => Serdes((IReadOnlyTexture<byte>)existing, info, mapping, s, jsonUtil);

        public IReadOnlyTexture<byte> Serdes(IReadOnlyTexture<byte> existing, AssetInfo info, AssetMapping mapping, ISerializer s, IJsonUtil jsonUtil)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (info == null) throw new ArgumentNullException(nameof(info));

            if (s.IsWriting())
            {
                if (existing == null) throw new ArgumentNullException(nameof(existing));
                Write(existing, s);
                return existing;
            }

            return Read(info, s);
        }

        static IReadOnlyTexture<byte> Read(AssetInfo info, ISerializer s)
        {
            ushort width = s.UInt16("Width", 0);
            ushort height = s.UInt16("Height", 0);
            int something = s.UInt8(null, 0);
            ApiUtil.Assert(something == 0);
            byte frameCount = s.UInt8("Frames", 1);

            var result = new SimpleTexture<byte>(info.AssetId, width, height * frameCount);
            for (int i = 0; i < frameCount; i++)
            {
                byte[] frameBytes = s.Bytes("Frame" + i, null, width * height);
                result.AddRegion(0, i * height, width, height);
                BlitUtil.BlitDirect(
                    new ReadOnlyImageBuffer<byte>(width, height, width, frameBytes),
                    result.GetMutableRegionBuffer(i));
            }

            s.Check();
            return result;
        }

        static void Write(IReadOnlyTexture<byte> existing, ISerializer s)
        {
            var distinctSizes = existing.Regions.Select(x => (x.Width, x.Height)).Distinct();
            if (distinctSizes.Count() > 1)
            {
                var parts = distinctSizes.Select(x => $"({x.Width}, {x.Height})");
                var joined = string.Join(", ", parts);
                throw new InvalidOperationException($"Tried to a write an image with non-uniform frames to a single-header sprite (sizes: {joined})");
            }

            var width = (ushort)existing.Regions[0].Width;
            var height = (ushort)existing.Regions[0].Height;
            var frameCount = (byte)existing.Regions.Count;

            s.UInt16("Width", width);
            s.UInt16("Height", height);
            s.UInt8(null, 0);
            s.UInt8("Frames", frameCount);

            var frameBytes = new byte[width * height];
            var frame = new ImageBuffer<byte>(width, height, width, frameBytes);
            for (int i = 0; i < frameCount; i++)
            {
                BlitUtil.BlitDirect(existing.GetRegionBuffer(i), frame);
                s.Bytes("Frame" + i, frameBytes, width * height);
            }
        }
    }
}
