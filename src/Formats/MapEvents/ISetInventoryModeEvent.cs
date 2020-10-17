using UAlbion.Api;
using UAlbion.Config;

namespace UAlbion.Formats.MapEvents
{
    public interface ISetInventoryModeEvent : IEvent
    {
        InventoryMode Mode { get; }
        AssetId Submode { get; }
    }
}
