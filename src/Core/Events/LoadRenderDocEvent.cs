using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("e:load_renderdoc")] public class LoadRenderDocEvent : EngineEvent { }