namespace UAlbion.Core
{
    public abstract class ServiceComponent<T> : Component
    {
        protected override void Subscribed()
        {
            Exchange.Register(typeof(T), this);
        }

        protected override void Unsubscribed()
        {
            Exchange.Unregister(typeof(T), this);
        }
    }
}
