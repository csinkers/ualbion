using System;
using UAlbion.Core.Entities;

namespace UAlbion.Core.Gui
{
    class InventoryButton : GuiElement
    {
        Func<Item> _itemGetter;
    }
}