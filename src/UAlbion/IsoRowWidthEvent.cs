using UAlbion.Api;

namespace UAlbion
{
    [Event("iso_row_width")]
    public class IsoRowWidthEvent : Event
    {
        public IsoRowWidthEvent(int delta) => Delta = delta;
        [EventPart("delta")] public int Delta { get; }
    }
}