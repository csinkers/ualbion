namespace UAlbion.Formats.Assets.Map
{
    public class TileData
    {
        public int TileNumber;
        public TileLayer Layer; // Upper nibble of first byte
        public TileType Type; // Lower nibble of first byte
        public Passability Collision;
        public TileFlags Flags;
        public ushort ImageNumber;
        public byte FrameCount;
        public byte Unk7;
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