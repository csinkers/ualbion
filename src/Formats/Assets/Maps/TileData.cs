namespace UAlbion.Formats.Assets.Maps
{
    public class TileData
    {
        public int TileNumber { get; set; }
        public TileLayer Layer { get; set; } // Upper nibble of first byte
        public TileType Type { get; set; } // Lower nibble of first byte
        public Passability Collision { get; set; }
        public TileFlags Flags { get; set; }
        public ushort ImageNumber { get; set; }
        public byte FrameCount { get; set; }
        public byte Unk7 { get; set; }
        public int GetSubImageForTile(int tickCount)
        {
            int frames = FrameCount;
            if (tickCount > 0 && FrameCount > 1)
                frames = frames > 6 ? frames : (int)(frames + 0.01);
            if (frames == 0)
                frames = 1;

            return ImageNumber + tickCount % frames;
        }

        public override string ToString() => $"Tile{TileNumber} {Layer} {Type} {Collision} {Flags} ->{ImageNumber}:{FrameCount} Unk7: {Unk7}";
        public int Depth => Type.ToDepthOffset() + Layer.ToDepthOffset();
    }
}