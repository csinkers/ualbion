using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("e:recreate_window")] public class RecreateWindowEvent : EngineEvent { }