using UAlbion.Api.Eventing;
using UAlbion.Core.Events;

namespace UAlbion.Core.Veldrid.Events;

[Event("e:focus_lost")] public class FocusLostEvent : EngineEvent { }