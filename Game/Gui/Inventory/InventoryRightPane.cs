using UAlbion.Formats.AssetIds;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryRightPane : UiElement
    {
        const int InventoryWidth = 4;
        const int InventoryHeight = 6;
        const string GoldButtonId = "Inventory.Gold";
        const string FoodButtonId = "Inventory.Food";
        const string GoldSummaryButtonId = "Inventory.GoldSummary";

        int _version;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<InventoryRightPane, InventoryChangedEvent>((x, e) => x._version++)
        );

        public InventoryRightPane(PartyCharacterId activeCharacter, string exitButtonId, bool showTotalPartyGold)
            : base(Handlers)
        {
            var header = new Header(new StringId(AssetType.SystemText, 0, (int)SystemTextId.Inv_Backpack));

            var slotSpans = new IUiElement[InventoryHeight];
            for (int j = 0; j < InventoryHeight; j++)
            {
                var slotsInRow = new IUiElement[InventoryWidth];
                for (int i = 0; i < InventoryWidth; i++)
                {
                    int index = j * InventoryWidth + i;
                    slotsInRow[i] = new InventoryBackpackSlot(activeCharacter, index);
                }
                slotSpans[j] = new HorizontalStack(slotsInRow);
            }

            var slotStack = new VerticalStack(slotSpans);
            //var slotFrame = new ButtonFrame(slotStack) { State = ButtonState.Pressed, Theme = new FrameTheme() };

            HorizontalStack moneyAndFoodStack;
            if (showTotalPartyGold)
            {
                var money = new Button(GoldSummaryButtonId,
                    new VerticalStack(
                        new Padding(64, 0),
                        new UiSprite<CoreSpriteId>(CoreSpriteId.UiGold) { Highlighted = true },
                        new Text("Total party gold $10.0")
                    ) { Greedy = false}
                ) { IsPressed = true };
                moneyAndFoodStack = new HorizontalStack(money);
            }
            else
            {
                var goldSource = new DynamicText(() =>
                {
                    var player = Resolve<IParty>()[activeCharacter];
                    var gold = player?.Apparent.Inventory.Gold ?? 0;
                    return new[] {new TextBlock($"{gold / 10}.{gold % 10}")};
                }, x => _version);

                var goldButton = new Button(GoldButtonId,
                    new VerticalStack(
                        new Padding(31, 0),
                        new UiSprite<CoreSpriteId>(CoreSpriteId.UiGold),
                        new Text(goldSource)
                    ) { Greedy = false });

                var foodSource = new DynamicText(() =>
                {
                    var player = Resolve<IParty>()[activeCharacter];
                    var food = player?.Apparent.Inventory.Rations ?? 0;
                    return new[] { new TextBlock(food.ToString()) };
                }, x => _version);

                var foodButton = new Button(FoodButtonId,
                    new VerticalStack(
                        new Padding(31, 0),
                        new UiSprite<CoreSpriteId>(CoreSpriteId.UiFood),
                        new Text(foodSource)
                    ) { Greedy = false });
                moneyAndFoodStack = new HorizontalStack(goldButton, foodButton);
            }

            var stack = new VerticalStack(
                header,
                slotStack, // slotFrame,
                new Padding(0, 2),
                moneyAndFoodStack,
                new Padding(0, 9),
                new InventoryExitButton(exitButtonId)
            ) { Greedy = false };

            Children.Add(stack);
        }
    }
}