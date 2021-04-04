using UAlbion.Api;

namespace UAlbion
{
    [Event("iso_width")]
    public class IsoWidthEvent : Event
    {
        public IsoWidthEvent(int delta) => Delta = delta;
        [EventPart("delta")] public int Delta { get; }
    }
}