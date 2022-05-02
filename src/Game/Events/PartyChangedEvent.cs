using UAlbion.Api.Eventing;

namespace UAlbion.Game.Events;

[Event("party_changed", "Emitted when party members have been added or removed")]
public class PartyChangedEvent : GameEvent
{
}