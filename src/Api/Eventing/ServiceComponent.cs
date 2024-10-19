namespace UAlbion.Api.Eventing;

public abstract class ServiceComponent<T>(bool registerAsService = true) : Component
{
    protected override void Subscribing()
    {
        if (registerAsService)
            Exchange.Register(typeof(T), this, false);
    }

    protected override void Unsubscribed()
    {
        if (registerAsService)
            Exchange.Unregister(typeof(T), this);
    }
}

public abstract class ServiceComponent<TFirst, TSecond>(bool registerAsService = true) : Component
{
    protected override void Subscribing()
    {
        if (!registerAsService) return;
        Exchange.Register(typeof(TFirst), this, false);
        Exchange.Register(typeof(TSecond), this, false);
    }

    protected override void Unsubscribed()
    {
        if (!registerAsService) return;
        Exchange.Unregister(typeof(TSecond), this);
        Exchange.Unregister(typeof(TFirst), this);
    }
}