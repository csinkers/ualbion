namespace UAlbion.Api.Eventing;

public abstract class ServiceComponent<T> : Component
{
    protected override void Subscribing() => Exchange.Register(typeof(T), this, false);
    protected override void Unsubscribed() => Exchange.Unregister(typeof(T), this);
}

public abstract class ServiceComponent<TFirst, TSecond> : Component
{
    protected override void Subscribing()
    {
        Exchange.Register(typeof(TFirst), this, false);
        Exchange.Register(typeof(TSecond), this, false);
    }

    protected override void Unsubscribed()
    {
        Exchange.Unregister(typeof(TSecond), this);
        Exchange.Unregister(typeof(TFirst), this);
    }
}