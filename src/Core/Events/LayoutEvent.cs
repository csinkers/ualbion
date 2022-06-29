using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("eng:layout")]
public class LayoutEvent : EngineEvent, IVerboseEvent { }