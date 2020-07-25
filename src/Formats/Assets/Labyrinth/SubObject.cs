using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.Assets.Labyrinth
{
    public class SubObject
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Z { get; set; }
        public ushort ObjectInfoNumber { get; set; }
        public override string ToString() => $"{ObjectInfoNumber}({ObjectId}) @ ({X}, {Y}, {Z})";
        internal DungeonObjectId? ObjectId { get; set; }
    }
}
