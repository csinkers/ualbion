using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("blur")] public class BlurEvent : CancellableEvent { }