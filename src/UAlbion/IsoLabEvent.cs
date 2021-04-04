using UAlbion.Api;

namespace UAlbion
{
    [Event("iso_lab")]
    public class IsoLabEvent : Event, IVerboseEvent
    {
        public IsoLabEvent(int delta) => Delta = delta;
        [EventPart("delta")] public int Delta { get; }
    }
}