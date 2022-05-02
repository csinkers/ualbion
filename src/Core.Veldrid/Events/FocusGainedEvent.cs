using UAlbion.Api.Eventing;
using UAlbion.Core.Events;

namespace UAlbion.Core.Veldrid.Events;

[Event("e:focus_gained")] public class FocusGainedEvent : EngineEvent { }