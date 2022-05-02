namespace UAlbion.Api.Eventing;

public abstract class CancellableEvent : Event, ICancellableEvent
{
    public bool Propagating { get; set; } = true;
}