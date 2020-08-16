using UAlbion.Api;

namespace UAlbion.Core
{
    public interface IComponent
    {
        void Attach(EventExchange exchange);
        void Remove();
        void Receive(IEvent e, object sender);
        bool IsActive { get; set; }
    }
}
