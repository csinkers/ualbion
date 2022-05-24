using UAlbion.Api.Eventing;
using UAlbion.Config;

namespace UAlbion.Game.Events;

[Event("asset:updated", "Emitted when an asset file change is detected to trigger hot-reloading")]
public class AssetUpdatedEvent : GameEvent
{
    public AssetUpdatedEvent(AssetId id) => Id = id;
    [EventPart("id")] public AssetId Id { get; }
}