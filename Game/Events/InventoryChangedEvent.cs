using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    public class InventoryChangedEvent : GameEvent, IPartyEvent, IVerboseEvent
    {
        public InventoryChangedEvent(PartyCharacterId memberId)
        {
            MemberId = memberId;
        }

        public PartyCharacterId MemberId { get; }
    }
}