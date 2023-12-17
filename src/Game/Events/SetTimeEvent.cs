using System;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

public class SetTimeEvent : GameEvent, IVerboseEvent
{
    public DateTime Time { get; set; }
}