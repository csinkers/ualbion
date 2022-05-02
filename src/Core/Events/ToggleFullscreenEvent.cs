using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("e:toggle_fullscreen")] public class ToggleFullscreenEvent : EngineEvent { }