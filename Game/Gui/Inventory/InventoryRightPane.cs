using System;
using System.Linq;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryRightPane : UiElement
    {
        const int InventoryWidth = 4;
        const int InventoryHeight = 6;

        public InventoryRightPane(PartyCharacterId activeCharacter, Action exitButtonCallback, bool showTotalPartyGold)
        {
            var header = new Header(new StringId(AssetType.SystemText, 0, (int)SystemTextId.Inv_Backpack));

            var slotSpans = new IUiElement[InventoryHeight];
            for (int j = 0; j < InventoryHeight; j++)
            {
                var slotsInRow = new IUiElement[InventoryWidth];
                for (int i = 0; i < InventoryWidth; i++)
                {
                    int index = j * InventoryWidth + i;
                    slotsInRow[i] = new LogicalInventorySlot(
                        InventoryType.Player,
                        (int)activeCharacter,
                        (ItemSlotId)((int)ItemSlotId.Slot0 + index));
                }
                slotSpans[j] = new HorizontalStack(slotsInRow);
            }

            var slotStack = new VerticalStack(slotSpans);
            //var slotFrame = new ButtonFrame(slotStack) { State = ButtonState.Pressed, Theme = new FrameTheme() };

            HorizontalStack moneyAndFoodStack;
            if (showTotalPartyGold)
            {
                var tf = Resolve<ITextFormatter>();
                int total = Resolve<IParty>().StatusBarOrder.Sum(x => x.Apparent.Inventory.Gold);
                var money = new Button(
                    new VerticalStack(
                        new Spacing(64, 0),
                        new UiSpriteElement<CoreSpriteId>(CoreSpriteId.UiGold) { Flags = SpriteFlags.Highlight },
                        new UiText(tf.Format(SystemTextId.Shop_GoldAll.ToId())),
                        new SimpleText($"{total / 10}.{total % 10}")
                    ) { Greedy = false})
                { IsPressed = true };
                moneyAndFoodStack = new HorizontalStack(money);
            }
            else
            {
                var goldButton = new LogicalInventorySlot(
                    InventoryType.Player,
                    (int)activeCharacter,
                    ItemSlotId.Gold);

                var foodButton = new LogicalInventorySlot(
                    InventoryType.Player,
                    (int)activeCharacter,
                    ItemSlotId.Rations);

                moneyAndFoodStack = new HorizontalStack(goldButton, foodButton);
            }

            var stack = new VerticalStack(
                header,
                slotStack, // slotFrame,
                new Spacing(0, 2),
                moneyAndFoodStack,
                new Spacing(0, 9),
                new InventoryExitButton(exitButtonCallback)
            ) { Greedy = false };

            AttachChild(stack);
        }
    }
}
