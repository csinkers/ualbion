using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryRightPane : UiElement
    {
        const int InventoryWidth = 4;
        const int InventoryHeight = 6;
        const string GoldButtonId = "Inventory.Gold";
        const string FoodButtonId = "Inventory.Food";
        const string GoldSummaryButtonId = "Inventory.GoldSummary";

        public InventoryRightPane(string exitButtonId, bool showTotalPartyGold)
        {
            var header = new Header(new StringId(AssetType.SystemText, 0, (int)SystemTextId.Inv_Backpack));

            var slotSpans = new IUiElement[InventoryHeight];
            for (int j = 0; j < InventoryHeight; j++)
            {
                var slotsInRow = new IUiElement[InventoryWidth];
                for (int i = 0; i < InventoryWidth; i++)
                {
                    int index = j * InventoryWidth + i;
                    slotsInRow[i] = new InventorySlot(index);
                }
                slotSpans[j] = new HorizontalStack(slotsInRow);
            }

            var slotStack = new VerticalStack(slotSpans);
            var slotFrame = new ButtonFrame(slotStack) { State = ButtonState.Pressed, Theme = new FrameTheme() };

            HorizontalStack moneyAndFoodStack;
            if (showTotalPartyGold)
            {
                var money = new ImageButton(GoldSummaryButtonId, "Total party gold $10.0") { IsPressed = true };
                moneyAndFoodStack = new HorizontalStack(money);
            }
            else
            {
                var money = new ImageButton(GoldButtonId, "$0.0");
                var food = new ImageButton(FoodButtonId, "FOOD");
                moneyAndFoodStack = new HorizontalStack(money, food);
            }

            var stack = new VerticalStack(
                header,
                slotFrame,
                moneyAndFoodStack,
                 new InventoryExitButton(exitButtonId)
            );

            Children.Add(stack);
        }
    }
}