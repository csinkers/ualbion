using UAlbion.Api.Eventing;
using UAlbion.Core.Events;

namespace UAlbion.Core.Veldrid.Events;

[Event("e:confine_mouse")]
public class ConfineMouseToWindowEvent : EngineEvent
{
    public ConfineMouseToWindowEvent(bool enabled) => Enabled = enabled;
    [EventPart("enabled", "True if mouse should be confined to the current window")] public bool Enabled { get; set; }
}