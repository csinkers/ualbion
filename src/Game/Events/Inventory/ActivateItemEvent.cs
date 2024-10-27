using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Inv;

namespace UAlbion.Game.Events.Inventory;

public class ActivateItemEvent : IEvent
{
    public ActivateItemEvent(InventorySlotId slotId) => SlotId = slotId;
    public InventorySlotId SlotId { get; }
    public void Format(IScriptBuilder builder) => builder?.Add(ScriptPartType.EventName, "inv:activate_item");
}