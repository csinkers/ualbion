using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core
{
    public interface IComponent
    {
        void Attach(EventExchange exchange);
        void Receive(IEvent @event, object sender);
        void Detach();
    }
}
