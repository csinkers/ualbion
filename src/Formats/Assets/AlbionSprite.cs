using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.Assets
{
    public class AlbionSprite2 : IEightBitImage
    {
        public AlbionSprite2(AssetId id, int width, int height, bool uniformFrames, byte[] pixelData, IEnumerable<AlbionSpriteFrame> frames)
        {
            AssetId = id;
            Width = width;
            Height = height;
            UniformFrames = uniformFrames;
            PixelData = pixelData;
            Frames = frames.ToArray();
        }

        public ITextureId Id => AssetId;
        public string Name => AssetId.ToString();
        public AssetId AssetId { get; }
        public int Width { get; }
        public int Height { get; }
        public int SubImageCount => Frames.Count;
        public int SizeInBytes => PixelData.Length;
        public ISubImage GetSubImage(int subImage) => subImage < Frames.Count ? Frames[subImage] : null;

        public bool UniformFrames { get; }
        public IReadOnlyList<AlbionSpriteFrame> Frames { get; }
        public byte[] PixelData { get; }
        ReadOnlySpan<byte> IEightBitImage.PixelData => PixelData;

        public override string ToString() => $"AlbionSprite {Id} {Width}x{Height} ({Frames.Count} frames)";

        public ReadOnlySpan<byte> GetRowSpan(int frameNumber, int row)
        {
            if(frameNumber >= Frames.Count)
                throw new ArgumentOutOfRangeException(nameof(frameNumber), $"Tried to get span for frame {frameNumber}, but the image only has {Frames.Count} frames");

            var frame = Frames[frameNumber];
            if (row >= frame.Height)
                throw new ArgumentOutOfRangeException(nameof(row), $"Tried to get span for row {row}, but the frame only has a height of {frame.Height}");
            int index = frame.X + Width * (frame.Y + row);
            return PixelData.AsSpan(index, frame.Width);
        }
    }
}
