using System;
using UAlbion.Api;

namespace UAlbion.Game.Events;

public class SetTimeEvent : GameEvent, IVerboseEvent
{
    public SetTimeEvent(DateTime time) => Time = time;
    public DateTime Time { get; }
}