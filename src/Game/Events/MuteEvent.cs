using UAlbion.Api;

namespace UAlbion.Game.Events;

[Event("mute", "Stop playing all active sounds")]
public class MuteEvent : GameEvent { }