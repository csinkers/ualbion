using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("assets:cycle")]
public class CycleCacheEvent : GameEvent, IVerboseEvent { }