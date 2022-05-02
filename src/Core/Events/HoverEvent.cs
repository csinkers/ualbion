using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("hover")] public class HoverEvent : CancellableEvent {}