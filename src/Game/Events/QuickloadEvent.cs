using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("quickload", "Quick load the game from the dedicated quicksave slot")]
public class QuickloadEvent : GameEvent
{
    public QuickloadEvent() { }
}
