using UAlbion.Api;
using UAlbion.Game.Gui;

namespace UAlbion.Game.Events
{
    [Event("set_inv_mode", "Sets the current inventory mode")]
    public class SetInventoryModeEvent : GameEvent
    {
        public SetInventoryModeEvent(InventoryMode mode) { Mode = mode; }
        [EventPart("mode")] public InventoryMode Mode { get; }
    }
}