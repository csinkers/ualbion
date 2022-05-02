using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;

namespace UAlbion.Formats.MapEvents;

public interface ITextEvent : IEvent
{
    TextId TextSource { get; }
}