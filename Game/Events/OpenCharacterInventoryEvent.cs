using UAlbion.Api;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Events
{
    [Event("open_character_inventory", "Opens the inventory screen for the given character")]
    public class OpenCharacterInventoryEvent : GameEvent, IPartyEvent
    {
        public OpenCharacterInventoryEvent(PartyCharacterId memberId)
        {
            MemberId = memberId;
        }

        [EventPart("memberid")]
        public PartyCharacterId MemberId { get; }
    }
}