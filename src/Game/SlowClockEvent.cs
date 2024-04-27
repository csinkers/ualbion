using UAlbion.Api.Eventing;
using UAlbion.Game.Events;

namespace UAlbion.Game;

[Event("slow_clock")] public class SlowClockEvent : GameEvent, IVerboseEvent { }