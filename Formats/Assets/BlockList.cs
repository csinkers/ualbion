namespace UAlbion.Formats.Assets
{
    public class Block
    {
        public int[] Underlay { get; set; }
        public int[] Overlay { get; set; }

        public byte Width { get; set; }
        public byte Height { get; set; }
        public override string ToString() => $"BLK {Width}x{Height}";
    }
}