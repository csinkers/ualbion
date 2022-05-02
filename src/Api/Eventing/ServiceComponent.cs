namespace UAlbion.Api.Eventing;

public abstract class ServiceComponent<T> : Component
{
    protected override void Subscribing() => Exchange.Register(typeof(T), this, false);
    protected override void Unsubscribed() => Exchange.Unregister(typeof(T), this);
}