using UAlbion.Api.Eventing;
using UAlbion.Formats.Assets.Inv;

namespace UAlbion.Game.Events.Inventory;

public class ReadSpellScrollEvent : IEvent
{
    public ReadSpellScrollEvent(InventorySlotId slotId) => SlotId = slotId;
    public InventorySlotId SlotId { get; }
    public void Format(IScriptBuilder builder) => builder?.Add(ScriptPartType.EventName, "inv:read_spell_scroll");
}