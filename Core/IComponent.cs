using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core
{
    public interface IComponent
    {
        void Attach(EventExchange exchange);
        void Detach();
        void Receive(IEvent @event, object sender);
        void Subscribed(); // Should only be called by EventExchange
        bool IsSubscribed { get; }
    }
}
