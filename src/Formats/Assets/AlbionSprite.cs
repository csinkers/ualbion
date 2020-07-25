using System.Collections.Generic;

namespace UAlbion.Formats.Assets
{
    public class AlbionSprite
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public IList<Frame> Frames { get; set; }
        public bool UniformFrames { get; set; }
        public byte[] PixelData { get; set; }

        public class Frame
        {
            public Frame(int x, int y, int width, int height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }

            public int X { get; }
            public int Y { get; }
            public int Width { get; }
            public int Height { get; }
        }

        public override string ToString() => $"AlbionSprite {Name} {Width}x{Height} ({Frames.Count} frames)";
    }
}
