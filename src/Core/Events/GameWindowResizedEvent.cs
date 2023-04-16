using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("e:game_window_resized")]
public class GameWindowResizedEvent : EngineEvent, IVerboseEvent { }