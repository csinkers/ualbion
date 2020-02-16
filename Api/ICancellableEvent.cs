namespace UAlbion.Api
{
    public interface ICancellableEvent : IEvent
    {
        bool Propagating { get; set; }
    }
}