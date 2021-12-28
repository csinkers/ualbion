using UAlbion.Api;

namespace UAlbion.Core.Events;

[Event("e:gc", "Force a Gen3 garbage collection")]
public class GarbageCollectionEvent : EngineEvent { }