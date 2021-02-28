using System;
using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Formats.Assets
{
    public class AlbionSprite
    {
        public AlbionSprite(string name, int width, int height, bool uniformFrames, byte[] pixelData, IEnumerable<AlbionSpriteFrame> frames)
        {
            Name = name;
            Width = width;
            Height = height;
            UniformFrames = uniformFrames;
            PixelData = pixelData;
            Frames = frames.ToArray();
        }

        public string Name { get; }
        public int Width { get; }
        public int Height { get; }
        public bool UniformFrames { get; }
        public IReadOnlyList<AlbionSpriteFrame> Frames { get; }
        public byte[] PixelData { get; }

        public override string ToString() => $"AlbionSprite {Name} {Width}x{Height} ({Frames.Count} frames)";

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
