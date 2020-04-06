namespace UAlbion.Formats.Assets.Map
{
    public enum TileLayer : byte // Upper nibble of first byte
    {
        Normal = 0, // Most floors, low and centre EW walls
        Layer1 = 2, // Mid EW walls, Overlay1
        Layer2 = 4, // Overlay2
        Layer3 = 6, // NS walls + Overlay3
        Unk8 = 8,
        Unk10 = 10,
        Unk12 = 12,
        Unk14 = 14,

        Unused1 = 1,
        Unused3 = 3,
        Unused5 = 5,
        Unused7 = 7,
        Unused9 = 9,
        Unused11 = 11,
        Unused13 = 13,
        Unused15 = 15,
    }

    public static class TileLayerExtensions
    {
        public static int ToDepthOffset(this TileLayer layer)
        {
            int adjustment;
            switch ((int)layer & 0x7)
            {
                case (int)TileLayer.Normal: adjustment = 0; break;
                case (int)TileLayer.Layer1: adjustment = 1; break;
                case (int)TileLayer.Layer2: adjustment = 2; break;
                case (int)TileLayer.Layer3: adjustment = 8; break;
                default: adjustment = 0; break;
            }
            return adjustment;
        }
    }
}