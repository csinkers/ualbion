using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryChestPane : UiElement
    {
        const int InventoryWidth = 6;
        const int InventoryHeight = 4;

        readonly ChestId _id;
        int _version;

        public InventoryChestPane(ChestId id)
        {
            On<InventoryChangedEvent>(e => _version++);

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
                    slotsInRow[i] = new LogicalInventorySlot(new InventorySlotId(
                        InventoryType.Chest,
                        (ushort)_id,
                        (ItemSlotId)((int)ItemSlotId.Slot0 + index)));
                }
                slotSpans[j] = new HorizontalStack(slotsInRow);
            }

            var slotStack = new VerticalStack(slotSpans);
            //var slotFrame = new ButtonFrame(slotStack) { State = ButtonState.Pressed, Theme = new FrameTheme() };

            var goldButton = new LogicalInventorySlot(new InventorySlotId(
                InventoryType.Chest,
                (ushort)_id,
                ItemSlotId.Gold));

            var foodButton = new LogicalInventorySlot(new InventorySlotId(
                InventoryType.Chest,
                (ushort)_id,
                ItemSlotId.Rations));

            var takeAllButton =
                new Button(
                (UiElement)new UiTextBuilder(UAlbionStringId.TakeAll).Center()
                ).OnClick(() => Raise(new InventoryTakeAllEvent(_id)));

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
