using System.Linq;
using UAlbion.Core.Visual;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.State;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory;

public class InventoryRightPane : UiElement
{
    const int InventoryWidth = 4;
    const int InventoryHeight = 6;

#pragma warning disable CA1506 // '.ctor' is coupled with '41' different types from '15' different namespaces. Rewrite or refactor the code to decrease its class coupling below '41'.
    public InventoryRightPane(PartyMemberId activeCharacter, bool showTotalPartyGold)
    {
        var header = new Header(Base.SystemText.Inv_Backpack);

        var sheetId = activeCharacter.ToSheet();
        var slotSpans = new IUiElement[InventoryHeight];
        for (int j = 0; j < InventoryHeight; j++)
        {
            var slotsInRow = new IUiElement[InventoryWidth];
            for (int i = 0; i < InventoryWidth; i++)
            {
                int index = j * InventoryWidth + i;
                slotsInRow[i] = new LogicalInventorySlot(new InventorySlotId(sheetId, (ItemSlotId)((int)ItemSlotId.Slot0 + index)));
            }
            slotSpans[j] = new HorizontalStacker(slotsInRow);
        }

        var slotStack = new VerticalStacker(slotSpans);
        var slotHalfFrame = new ButtonFrame(slotStack) {Theme = ButtonTheme.InventoryOuterFrame, Padding = -1 };

        HorizontalStacker moneyAndFoodStacker;
        if (showTotalPartyGold)
        {
            var tf = Resolve<ITextFormatter>();
            int total = Resolve<IParty>().StatusBarOrder.Sum(x => x.Apparent.Inventory.Gold.Amount);
            var money = new Button(
                    new VerticalStacker(
                        new Spacing(64, 0),
                        new UiSpriteElement(Base.CoreGfx.UiGold) { Flags = SpriteFlags.Highlight },
                        new UiText(tf.Format(Base.SystemText.Shop_GoldAll)),
                        new SimpleText($"{total / 10}.{total % 10}")
                    ) { Greedy = false})
                { IsPressed = true };
            moneyAndFoodStacker = new HorizontalStacker(money);
        }
        else
        {
            var goldButton = new LogicalInventorySlot(new InventorySlotId(activeCharacter, ItemSlotId.Gold));
            var foodButton = new LogicalInventorySlot(new InventorySlotId(activeCharacter, ItemSlotId.Rations));
            moneyAndFoodStacker = new HorizontalStacker(goldButton, foodButton);
        }

        var stack = new FixedWidth(77, new VerticalStacker(
            new Spacing(0, 1),
            new Greedy(header),
            new Spacing(0, 1),
            slotHalfFrame,
            new Spacing(0, 2),
            moneyAndFoodStacker,
            new Spacing(0, 9),
            new InventoryExitButton().OnClick(() => Raise(new InventoryCloseEvent()))
        ) { Greedy = false });

        AttachChild(stack);
    }
#pragma warning restore CA1506 // '.ctor' is coupled with '41' different types from '15' different namespaces. Rewrite or refactor the code to decrease its class coupling below '41'.
}