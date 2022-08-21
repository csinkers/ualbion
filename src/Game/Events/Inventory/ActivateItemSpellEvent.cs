using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets;

namespace UAlbion.Game.Events.Inventory;

public class ActivateItemSpellEvent : IEvent
{
    public ActivateItemSpellEvent(InventorySlotId slotId) => SlotId = slotId;
    public InventorySlotId SlotId { get; }
    public void Format(IScriptBuilder builder) => builder?.Add(ScriptPartType.EventName, "inv:activate_item_spell");
}