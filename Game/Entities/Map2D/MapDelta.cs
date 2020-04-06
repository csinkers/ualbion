namespace UAlbion.Game.Entities.Map2D
{
    public class MapDelta
    {
        public enum DeltaType
        {
            Underlay,
            Overlay,
            Chain,
            Trigger,
        }

        public DeltaType Type { get; set; }
        public bool Permanent { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
    }
}