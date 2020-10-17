using UAlbion.Api;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents
{
    public interface ITextEvent : IEvent
    {
        TextId TextSourceId { get; }
    }
}
