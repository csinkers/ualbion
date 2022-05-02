using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

[Event("quit", "Exit the game.", "exit")]
public class QuitEvent : EngineEvent { }