using UAlbion.Api;

namespace UAlbion.Core
{
    public interface IComponent
    {
        void Attach(EventExchange exchange);
        void Remove();
        void Receive(IEvent @event, object sender);
        bool IsActive { get; set; }
    }
}
