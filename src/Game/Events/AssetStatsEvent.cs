using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("assets:stats", "Print asset cache statistics.")]
public class AssetStatsEvent : GameEvent { }