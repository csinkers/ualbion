using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("load_game", "Load a saved game")]
public class LoadGameEvent : GameEvent
{
    public LoadGameEvent(ushort id) => Id = id;

    [EventPart("id", "The slot number to load from")]
    public ushort Id { get; }
}