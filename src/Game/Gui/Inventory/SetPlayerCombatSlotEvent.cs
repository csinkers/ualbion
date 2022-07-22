using UAlbion.Api.Eventing;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Gui.Inventory;

[Event("set_combat_slot")]
public class SetPlayerCombatSlotEvent : Event
{
    public SetPlayerCombatSlotEvent(PartyMemberId id, int slot)
    {
        Id = id;
        Slot = slot;
    }

    [EventPart("id")] public PartyMemberId Id { get; }
    [EventPart("slot")] public int Slot { get; }
}