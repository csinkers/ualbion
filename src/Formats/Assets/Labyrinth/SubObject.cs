namespace UAlbion.Formats.Assets.Labyrinth
{
    public class SubObject
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
        public ushort ObjectInfoNumber { get; set; }
        public override string ToString() => $"{ObjectInfoNumber}({SpriteId}) @ ({X}, {Y}, {Z})";
        internal SpriteId SpriteId { get; set; }
    }
}
