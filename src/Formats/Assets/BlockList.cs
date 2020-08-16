namespace UAlbion.Formats.Assets
{
    public class Block
    {
        readonly int[] _underlay;
        readonly int[] _overlay;
        public Block(byte width, byte height, int[] underlay, int[] overlay)
        {
            Width = width;
            Height = height;
            _underlay = underlay;
            _overlay = overlay;
        }

        public byte Width { get; set; }
        public byte Height { get; set; }
        public int GetUnderlay(int index) => _underlay[index];
        public int GetOverlay(int index) => _overlay[index];
        public override string ToString() => $"BLK {Width}x{Height}";
    }
}
