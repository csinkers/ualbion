using UAlbion.Api;
using UAlbion.Config;
using UAlbion.Game.Text;

namespace UAlbion.Game.Events
{
    [Event("set_context")]
    public class SetContextEvent : GameEvent, IVerboseEvent
    {
        public SetContextEvent(ContextType type, AssetId assetId)
        {
            Type = type;
            AssetId = assetId;
        }

        [EventPart("type")] public ContextType Type { get; }
        [EventPart("assetid")] public AssetId AssetId { get; }
    }
}