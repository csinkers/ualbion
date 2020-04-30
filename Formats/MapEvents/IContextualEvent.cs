using UAlbion.Api;

namespace UAlbion.Formats.MapEvents
{
    public interface IContextualEvent : IEvent
    {
        EventContext Context { get; set; }
    }
}