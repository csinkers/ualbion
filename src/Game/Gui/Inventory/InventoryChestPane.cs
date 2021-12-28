using UAlbion.Formats.Assets;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.State;

namespace UAlbion.Game.Gui.Inventory;

public class InventoryChestPane : UiElement
{
    const int InventoryWidth = 6;
    const int InventoryHeight = 4;

    readonly ChestId _id;
    readonly UiSpriteElement _background;

    public InventoryChestPane(ChestId id)
    {
        On<InventoryChangedEvent>(e =>
        {
            if (e.Id != new InventoryId(_id))
                return;

            UpdateBackground();
        });

        _id = id;
        _background = new UiSpriteElement(Base.Picture.OpenChestWithGold);

        var backgroundStack = new FixedPositionStack();
        backgroundStack.Add(_background, 0, 0);
        AttachChild(backgroundStack);

        var slotSpans = new IUiElement[InventoryHeight];
        for (int j = 0; j < InventoryHeight; j++)
        {
            var slotsInRow = new IUiElement[InventoryWidth];
            for (int i = 0; i < InventoryWidth; i++)
            {
                int index = j * InventoryWidth + i;
                slotsInRow[i] = new LogicalInventorySlot(new InventorySlotId(_id, (ItemSlotId)((int)ItemSlotId.Slot0 + index)));
            }
            slotSpans[j] = new HorizontalStack(slotsInRow);
        }

        var slotStack = new VerticalStack(slotSpans);
        var slotHalfFrame = new ButtonFrame(slotStack) {Theme = ButtonTheme.InventoryOuterFrame, Padding = -1 };
        var goldButton = new LogicalInventorySlot(new InventorySlotId(_id, ItemSlotId.Gold));
        var foodButton = new LogicalInventorySlot(new InventorySlotId(_id, ItemSlotId.Rations));

        var takeAllButton =
            new Button(
                (UiElement)new UiTextBuilder(TextId.From(Base.UAlbionString.TakeAll)).Center()
            ).OnClick(() => Raise(new InventoryTakeAllEvent(_id)));

        var header = new Header(Base.SystemText.Chest_Chest);
        var moneyAndFoodStack = new HorizontalStack(goldButton, takeAllButton, foodButton);

        var stack = new VerticalStack(
            header,
            slotHalfFrame,
            new Spacing(0, 78),
            moneyAndFoodStack
        ) { Greedy = false };

        AttachChild(stack);
    }

    void UpdateBackground()
    {
        var inv = Resolve<IGameState>().GetInventory(new InventoryId(_id));
        _background.Id = inv.IsEmpty ? Base.Picture.OpenChest : Base.Picture.OpenChestWithGold;
    }

    protected override void Subscribed() => UpdateBackground();
}