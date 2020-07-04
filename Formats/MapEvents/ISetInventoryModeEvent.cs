using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Formats.MapEvents
{
    public interface ISetInventoryModeEvent : IEvent
    {
        InventoryMode Mode { get; }
        PartyCharacterId? Member { get; } // Default to party leader if null
        ISetInventoryModeEvent CloneForMember(PartyCharacterId member);
    }
}