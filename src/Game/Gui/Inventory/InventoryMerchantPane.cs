using UAlbion.Formats.Assets.Inv;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.State.Player;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory;

public class InventoryMerchantPane : UiElement
{
    const int InventoryWidth = 6;
    const int InventoryHeight = 4;
    int _queueVersion;

    public InventoryMerchantPane(MerchantId id, PartyMemberId activeCharacter)
    {
        On<SellQueueChangedEvent>(_ => _queueVersion++);

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
        var slotHalfFrame = new ButtonFrame(slotStack) { Theme = ButtonTheme.InventoryOuterFrame, Padding = -1 };
        var header = new Header(Base.SystemText.Shop_Merchant);

        // M6: Show active character's gold so the player knows their budget.
        var goldSlot = new LogicalInventorySlot(new InventorySlotId((InventoryId)activeCharacter, ItemSlotId.Gold));

        // QoL: Batch-sell button — shows queued count, sells all on click.
        var sellText = new DynamicText(() =>
        {
            var im = TryResolve<IInventoryManager>();
            int n = im?.SellQueue.Count ?? 0;
            return n == 0
                ? [new TextBlock("Sell All")]
                : [new TextBlock($"Sell All ({n})")];
        }, _ => _queueVersion);
        var sellButton = new Button(new UiText(sellText))
            .OnClick(() => TryResolve<IInventoryManager>()?.SellQueued());

        var stack = new VerticalStacker(header, slotHalfFrame, new Spacing(0, 4), goldSlot, new Spacing(0, 2), sellButton) { Greedy = false };
        AttachChild(stack);
    }
}