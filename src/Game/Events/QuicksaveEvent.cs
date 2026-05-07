using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("quicksave", "Quick save the game to a dedicated slot")]
public class QuicksaveEvent : GameEvent
{
    public QuicksaveEvent() { }
}
