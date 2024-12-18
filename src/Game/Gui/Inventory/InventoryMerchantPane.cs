﻿using UAlbion.Formats.Assets.Inv;
using UAlbion.Formats.Ids;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Gui.Inventory;

public class InventoryMerchantPane : UiElement
{
    const int InventoryWidth = 6;
    const int InventoryHeight = 4;

    public InventoryMerchantPane(MerchantId id)
    {
        var slotSpans = new IUiElement[InventoryHeight];
        for (int j = 0; j < InventoryHeight; j++)
        {
            var slotsInRow = new IUiElement[InventoryWidth];
            for (int i = 0; i < InventoryWidth; i++)
            {
                int index = j * InventoryWidth + i;
                slotsInRow[i] = new LogicalInventorySlot(new InventorySlotId(id, (ItemSlotId)((int)ItemSlotId.Slot0 + index)));
            }
            slotSpans[j] = new HorizontalStacker(slotsInRow);
        }

        var slotStack = new VerticalStacker(slotSpans);
        var slotHalfFrame = new ButtonFrame(slotStack) {Theme = ButtonTheme.InventoryOuterFrame, Padding = -1 };
        var header = new Header(Base.SystemText.Shop_Merchant);
        var stack = new VerticalStacker(header, slotHalfFrame) { Greedy = false };
        AttachChild(stack);
    }
}