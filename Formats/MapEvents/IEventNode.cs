using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public interface IEventNode
    {
        int Id { get; }
        IEvent Event { get; }
        IEventNode NextEvent { get; set; }
    }
}