using UAlbion.Api;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Text;

namespace UAlbion.Game.Events
{
    [Event("set_context")]
    public class SetContextEvent : GameEvent, IVerboseEvent
    {
        public SetContextEvent(ContextType type, AssetType assetType, int assetId)
        {
            Type = type;
            AssetType = assetType;
            AssetId = assetId;
        }

        [EventPart("type")] public ContextType Type { get; }
        [EventPart("asset_type")] public AssetType AssetType { get; }
        [EventPart("asset_id")] public int AssetId { get; }
    }
}