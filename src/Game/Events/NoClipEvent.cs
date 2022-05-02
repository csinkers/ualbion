﻿using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("noclip", "Toggles collision detection for the player(s)")]
public class NoClipEvent : GameEvent { }