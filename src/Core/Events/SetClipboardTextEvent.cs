using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("set_clipboard_text")]
public class SetClipboardTextEvent : Event, IVerboseEvent
{
    public SetClipboardTextEvent(string text) { Text = text; }
    [EventPart("text")] public string Text { get; }
}