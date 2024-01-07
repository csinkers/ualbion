using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("prompt:text")]
public class TextPromptEvent : Event, IQueryEvent<string>
{
}