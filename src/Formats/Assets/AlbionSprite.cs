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
    }
}
