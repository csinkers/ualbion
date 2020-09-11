using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public interface IEventNode
    {
        ushort Id { get; }
        IEvent Event { get; }
        IEventNode Next { get; }
    }
}
