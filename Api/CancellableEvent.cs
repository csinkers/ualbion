namespace UAlbion.Api
{
    public abstract class CancellableEvent : Event, ICancellableEvent
    {
        public bool Propagating { get; set; } = true;
    }
}