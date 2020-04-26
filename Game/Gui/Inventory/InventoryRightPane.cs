using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;
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
                    slotsInRow[i] = new InventorySlot(
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
                var money = new Button(
                    new VerticalStack(
                        new Spacing(64, 0),
                        new UiSpriteElement<CoreSpriteId>(CoreSpriteId.UiGold) { Highlighted = true },
                        new TextElement("Total party gold $10.0")
                    ) { Greedy = false}, () => { } // TODO: Make button functional
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

                var goldButton = new Button(
                    new VerticalStack(
                        new Spacing(31, 0),
                        new UiSpriteElement<CoreSpriteId>(CoreSpriteId.UiGold),
                        new TextElement(goldSource)
                    ) { Greedy = false }, () => { } // TODO: Make button functional
                );

                var foodSource = new DynamicText(() =>
                {
                    var player = Resolve<IParty>()[activeCharacter];
                    var food = player?.Apparent.Inventory.Rations ?? 0;
                    return new[] { new TextBlock(food.ToString()) };
                }, x => _version);

                var foodButton = new Button(
                    new VerticalStack(
                        new Spacing(31, 0),
                        new UiSpriteElement<CoreSpriteId>(CoreSpriteId.UiFood),
                        new TextElement(foodSource)
                    ) { Greedy = false }, () => { } // TODO: Make button functional
                );
                moneyAndFoodStack = new HorizontalStack(goldButton, foodButton);
            }

            var stack = new VerticalStack(
                header,
                slotStack, // slotFrame,
                new Spacing(0, 2),
                moneyAndFoodStack,
                new Spacing(0, 9),
                new InventoryExitButton(exitButtonId)
            ) { Greedy = false };

            AttachChild(stack);
        }
    }
}
