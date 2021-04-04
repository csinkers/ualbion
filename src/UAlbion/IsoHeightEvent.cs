using UAlbion.Api;

namespace UAlbion
{
    [Event("iso_height")]
    public class IsoHeightEvent : Event
    {
        public IsoHeightEvent(int delta) => Delta = delta;
        [EventPart("delta")] public int Delta { get; }
    }
}