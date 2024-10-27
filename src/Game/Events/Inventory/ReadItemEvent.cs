using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Inv;

namespace UAlbion.Game.Events.Inventory;

public class ReadItemEvent : IEvent
{
    public InventorySlotId SlotId { get; }
    public ReadItemEvent(InventorySlotId slotId) => SlotId = slotId;
    public void Format(IScriptBuilder builder) => builder?.Add(ScriptPartType.EventName, "inv:read_item");
}