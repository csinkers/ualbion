using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Formats.MapEvents;

public interface ITextEvent : IEvent
{
    TextId TextSource { get; }
}