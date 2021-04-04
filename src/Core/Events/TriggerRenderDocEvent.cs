using UAlbion.Api;

namespace UAlbion.Core.Events
{
    [Event("trd", "Capture the next frame in RenderDoc", "e:trigger_renderdoc")] public class TriggerRenderDocEvent : EngineEvent { }
}