using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("mute", "Stop playing all active sounds")]
public class MuteEvent : GameEvent { }