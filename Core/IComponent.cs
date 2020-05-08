using UAlbion.Api;

namespace UAlbion.Core
{
    public interface IComponent
    {
        void Attach(EventExchange exchange);
        void Detach();
        void Receive(IEvent @event, object sender);
        bool IsActive { get; set; }
    }
}
