using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("pop_scene", "Restore a previously active scene")]
public class PopSceneEvent : GameEvent { }