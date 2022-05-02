using UAlbion.Api;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;

namespace UAlbion.Core.Veldrid.Events;

[Event("rebind")]
public class RebindInputEvent : EngineEvent { }