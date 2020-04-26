using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryChestPane : UiElement
    {
        const int InventoryWidth = 6;
        const int InventoryHeight = 4;

        readonly ChestId _id;
        int _version;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<InventoryChestPane, InventoryChangedEvent>((x, e) => x._version++)
        );

        public InventoryChestPane(ChestId id) : base(Handlers)
        {
            _id = id;

            var background = new FixedPositionStack();
            background.Add(new UiSpriteElement<PictureId>(PictureId.OpenChestWithGold), 0, 0);
            AttachChild(background);

            var slotSpans = new IUiElement[InventoryHeight];
            for (int j = 0; j < InventoryHeight; j++)
            {
                var slotsInRow = new IUiElement[InventoryWidth];
                for (int i = 0; i < InventoryWidth; i++)
                {
                    int index = j * InventoryWidth + i;
                    slotsInRow[i] = new InventorySlot(InventoryType.Chest, (int)_id, (ItemSlotId)((int)ItemSlotId.Slot0 + index));
                }
                slotSpans[j] = new HorizontalStack(slotsInRow);
            }

            var slotStack = new VerticalStack(slotSpans);
            //var slotFrame = new ButtonFrame(slotStack) { State = ButtonState.Pressed, Theme = new FrameTheme() };

            var goldSource = new DynamicText(() =>
            {
                var chest = Resolve<IGameState>().GetChest(_id);
                var gold = chest.Gold;
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
                var chest = Resolve<IGameState>().GetChest(_id);
                var food = chest.Rations;
                return new[] { new TextBlock(food.ToString()) };
            }, x => _version);

            var foodButton = new Button(
                new VerticalStack(
                    new Spacing(31, 0),
                    new UiSpriteElement<CoreSpriteId>(CoreSpriteId.UiFood),
                    new TextElement(foodSource)
                ) { Greedy = false }, () => { } // TODO: Make button functional
            );

            var takeAllButton = new Button(
                new TextElement(UAlbionStringId.TakeAll.ToId()),
                () => { }
            );

            var header = new Header(new StringId(AssetType.SystemText, 0, (int)SystemTextId.Chest_Chest));
            var moneyAndFoodStack = new HorizontalStack(goldButton, takeAllButton, foodButton);

            var stack = new VerticalStack(
                header,
                slotStack,
                new Spacing(0, 2),
                moneyAndFoodStack
            ) { Greedy = false };

            AttachChild(stack);
        }
    }
}
