using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Formats.Assets.Inv;

namespace UAlbion.Formats.Assets.Sheets
{
    public class PartyMemberInfo
    {
        public Dictionary<ItemSlotId, Position2D> InventorySlots { get; set; }
    }
}
