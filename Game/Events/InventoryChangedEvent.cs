using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    public class InventoryChangedEvent : GameEvent, IPartyEvent
    {
        public InventoryChangedEvent(PartyCharacterId memberId)
        {
            MemberId = memberId;
        }

        public PartyCharacterId MemberId { get; }
    }
}