using UAlbion.Api;
using UAlbion.Core.Events;

namespace UAlbion.Core.Veldrid.Events;

[Event("e:set_relative_mouse_mode")]
public class SetRelativeMouseModeEvent : EngineEvent
{
    public SetRelativeMouseModeEvent(bool enabled) => Enabled = enabled;
    [EventPart("enabled", "True if relative mouse mode should be enabled")] public bool Enabled { get; set; }
}