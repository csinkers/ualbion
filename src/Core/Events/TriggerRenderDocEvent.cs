using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("e:trigger_renderdoc", "Capture the next frame in RenderDoc", "trd")]
public class TriggerRenderDocEvent : EngineEvent { }