using UAlbion.Api;

namespace UAlbion.Game.Events;

[Event("assets:reload", "Flush the asset cache, forcing all data to be reloaded from disk")]
public class ReloadAssetsEvent : GameEvent { }