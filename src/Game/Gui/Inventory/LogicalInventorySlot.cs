using System.Collections.Generic;
using UAlbion.Config;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Input;
using UAlbion.Game.State;
using UAlbion.Game.State.Player;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory;

public class LogicalInventorySlot : UiElement
{
    readonly InventorySlotId _id;
    readonly VisualInventorySlot _visual;
    int _version = 1;

    public LogicalInventorySlot(InventorySlotId id)
    {
        On<InventoryChangedEvent>(e =>
        {
            if (e.Id == _id.Id)
                _version++;
        });

        _id = id;

        IText amountSource;
        if (id.Slot == ItemSlotId.Gold)
        {
            amountSource = new DynamicText(() =>
            {
                var gold = Inventory?.Gold.Amount ?? 0;
                return new[] { new TextBlock($"{gold / 10}.{gold % 10}") }; // todo: i18n: May need to vary based on the current game language
            }, _ => _version);
        }
        else if (id.Slot == ItemSlotId.Rations)
        {
            amountSource = new DynamicText(() =>
            {
                var food = Inventory?.Rations.Amount ?? 0;
                return new[] { new TextBlock(food.ToString()) }; // todo: i18n: Will need to be changed if we support a language that doesn't use Hindu-Arabic numerals.
            }, _ => _version);
        }
        else
        {
            amountSource = new DynamicText(() =>
            {
                var slotInfo = Slot;
                return slotInfo == null || slotInfo.Amount < 2
                    ? []
                    : new[] { new TextBlock(slotInfo.Amount.ToString()) { Alignment = TextAlignment.Right } }; // todo: i18n: Will need to be changed if we support a language that doesn't use Hindu-Arabic numerals.
            }, _ => _version);
        }

        _visual = AttachChild(new VisualInventorySlot(_id, amountSource, () => Slot))
            .OnButtonDown(() =>
            {
                var im = Resolve<IInventoryManager>();
                var inputBinder = Resolve<IInputBinder>();
                if (!im.ItemInHand.Item.IsNone
                    || inputBinder.IsCtrlPressed
                    || inputBinder.IsShiftPressed
                    || inputBinder.IsAltPressed)
                {
                    _visual!.SuppressNextDoubleClick = true;
                }
            })
            .OnClick(() =>
            {
                var inputBinder = Resolve<IInputBinder>();
                if (inputBinder.IsCtrlPressed)
                    Raise(new InventoryPickupEvent(null, _id.Id, _id.Slot));
                else if (inputBinder.IsShiftPressed)
                    Raise(new InventoryPickupEvent(5, _id.Id, _id.Slot));
                else if (inputBinder.IsAltPressed)
                    Raise(new InventoryPickupEvent(1, _id.Id, _id.Slot));
                else
                    Raise(new InventorySwapEvent(_id.Id, _id.Slot));
            })
            .OnDoubleClick(() => Raise(new InventoryPickupEvent(null, _id.Id, _id.Slot)))
            .OnRightClick(OnRightClick)
            .OnHover(Hover)
            .OnBlur(Blur);
    }

    public override string ToString() => $"InventorySlot:{_id}";
    IInventory Inventory => Resolve<IGameState>().GetInventory(_id.Id);
    IReadOnlyItemSlot Slot => Inventory?.GetSlot(_id.Slot);

    void Blur()
    {
        var inventoryManager = Resolve<IInventoryManager>();
        var hand = inventoryManager.ItemInHand;
        Raise(new SetCursorEvent(hand.Item.IsNone ? Base.CoreGfx.Cursor : Base.CoreGfx.CursorSmall));
        Raise(new HoverTextEvent(null));
    }

    void Hover()
    {
        var inventoryManager = Resolve<IInventoryManager>();
        var inventory = Resolve<IGameState>().GetInventory(_id.Id);
        var tf = Resolve<ITextFormatter>();

        var slotInfo = Slot;
        string itemName = null;
        if (slotInfo?.Item.Type == AssetType.Item)
        {
            var item = Assets.LoadItem(slotInfo.Item);
            itemName = Assets.LoadStringSafe(item.Name);
        }

        var hand = inventoryManager.ItemInHand;
        string itemInHandName = null;
        if (hand.Item.Type == AssetType.Item)
        {
            var itemInHand = Assets.LoadItem(hand.Item);
            itemInHandName = Assets.LoadStringSafe(itemInHand.Name);
        }

        var action = inventoryManager.GetInventoryAction(_id);
        _visual.Hoverable = true;
        switch (action)
        {
            case InventoryAction.Nothing:
                _visual.Hoverable = false;
                break;
            case InventoryAction.Pickup: // <Item name>
            {
                if (itemName != null)
                {
                    Raise(new HoverTextEvent(new LiteralText(itemName)));
                    Raise(new SetCursorEvent(Base.CoreGfx.CursorSelected));
                }
                else if(_id.Slot is ItemSlotId.Gold or ItemSlotId.Rations)
                {
                    bool isGold = _id.Slot == ItemSlotId.Gold;
                    int amount = isGold ? inventory.Gold.Amount : inventory.Rations.Amount;
                    var text = isGold
                        ? tf.Format(Base.SystemText.Gold_NNGold, amount / 10, amount % 10)
                        : tf.Format(Base.SystemText.Gold_NRations, amount);
                    Raise(new HoverTextEvent(text));
                    Raise(new SetCursorEvent(Base.CoreGfx.CursorSelected));
                }
                break;
            }
            case InventoryAction.PutDown: // Put down %s
            {
                if (itemInHandName != null)
                {
                    var text = tf.Format(Base.SystemText.Item_PutDownX, itemInHandName);
                    Raise(new HoverTextEvent(text));
                }
                break;
            }
            case InventoryAction.Swap: // Swap %s with %s
            {
                if (itemInHandName != null && itemName != null)
                {
                    var text = tf.Format(Base.SystemText.Item_SwapXWithX, itemInHandName, itemName);
                    Raise(new HoverTextEvent(text));
                }
                break;
            }
            case InventoryAction.Coalesce: // Add
            {
                Raise(new HoverTextEvent(tf.Format(Base.SystemText.Item_Add)));
                break;
            }
            case InventoryAction.NoCoalesceFullStack: // {YELLOW}This space is occupied!
            {
                Raise(new HoverTextEvent(tf.Format(Base.SystemText.Item_ThisSpaceIsOccupied)));
                break;
            }
        }
    }

    void OnRightClick()
    {
        var inventory = Resolve<IGameState>().GetInventory(_id.Id);
        var slotInfo = inventory.GetSlot(_id.Slot);
        if (slotInfo?.Item.Type != AssetType.Item)
        {
            OnRightClickSpecial(slotInfo);
            return;
        }

        var tf = Resolve<ITextFormatter>();
        var window = Resolve<IGameWindow>();
        var cursorManager = Resolve<ICursorManager>();

        var item = Assets.LoadItem(slotInfo.Item);
        var itemPosition = window.UiToNorm(slotInfo.LastUiPosition);
        var heading = tf.Center().NoWrap().Fat().Format(item.Name);

        IText S(TextId textId, bool disabled = false)
            => tf
                .Center()
                .NoWrap()
                .Ink(disabled ? Base.Ink.Yellow : Base.Ink.White)
                .Format(textId);

        // Drop (Yellow inactive when critical)
        // Examine
        // Use (e.g. torch)
        // Drink
        // Activate (compass, clock, monster eye)
        // Activate spell (if has spell, yellow if combat spell & not in combat etc)
        // Read (e.g. metal-magic knowledge, maps)

        bool isPlotItem = (item.Flags & ItemFlags.PlotItem) != 0;
        var options = new List<ContextMenuOption>();

        if (_id.Id.Type == InventoryType.Merchant)
        {
            options.Add(new ContextMenuOption(
                S(Base.SystemText.InvPopup_Sell, isPlotItem),
                isPlotItem 
                    ? new HoverTextEvent(
                        tf.Format(
                            Base.SystemText.InvMsg_ThisIsAVitalItem))
                    : new InventorySellEvent(_id.Id, _id.Slot),
                ContextMenuGroup.Actions,
                isPlotItem));
        }
        else
        {
            options.Add(
                new ContextMenuOption(
                    S(Base.SystemText.InvPopup_Drop, isPlotItem),
                    isPlotItem
                        ? new HoverTextEvent(
                            tf.Format(
                                Base.SystemText.InvMsg_ThisIsAVitalItem))
                        : new InventoryDiscardEvent(itemPosition.X, itemPosition.Y, _id.Id, _id.Slot),
                    ContextMenuGroup.Actions,
                    isPlotItem));
        }

        options.Add(new ContextMenuOption(
            S(Base.SystemText.InvPopup_Examine),
            new InventoryExamineEvent(item.Id),
            ContextMenuGroup.Actions));

        if (item.TypeId == ItemType.Document && _id.Id.Type == InventoryType.Player)
        {
            options.Add(new ContextMenuOption(
                S(Base.SystemText.InvPopup_Read),
                new ReadItemEvent(_id),
                ContextMenuGroup.Actions));
        }

        if (item.TypeId == ItemType.SpellScroll && _id.Id.Type == InventoryType.Player)
        {
            options.Add(new ContextMenuOption(
                S(Base.SystemText.InvPopup_LearnSpell),
                new ReadSpellScrollEvent(_id),
                ContextMenuGroup.Actions));
        }

        if (item.TypeId == ItemType.Drink && _id.Id.Type == InventoryType.Player)
        {
            options.Add(new ContextMenuOption(
                S(Base.SystemText.InvPopup_Drink),
                new DrinkItemEvent(_id),
                ContextMenuGroup.Actions));
        }

        if (item.TypeId == ItemType.HeadsUpDisplayItem && _id.Id.Type == InventoryType.Player)
        {
            options.Add(new ContextMenuOption(
                S(Base.SystemText.InvPopup_Activate),
                new ActivateItemEvent(_id),
                ContextMenuGroup.Actions));
        }

        // TODO: Disable based on spell context
        if (item.Charges > 0 && _id.Id.Type == InventoryType.Player)
        {
            options.Add(new ContextMenuOption(
                S(Base.SystemText.InvPopup_ActivateSpell),
                new ActivateItemSpellEvent(_id),
                ContextMenuGroup.Actions));
        }

        var uiPosition = window.PixelToUi(cursorManager.Position);
        Raise(new ContextMenuEvent(uiPosition, heading, options));
    }

    void OnRightClickSpecial(IReadOnlyItemSlot slotInfo)
    {
        if (slotInfo.Item.IsNone)
            return;

        var tf = Resolve<ITextFormatter>();
        var window = Resolve<IGameWindow>();
        var cursorManager = Resolve<ICursorManager>();
        var headingText = slotInfo.Item == AssetId.Gold
            ? Base.SystemText.Gold_Gold 
            : Base.SystemText.Gold_Rations;

        var itemPosition = window.UiToNorm(slotInfo.LastUiPosition);
        var heading = tf.Center().NoWrap().Fat().Format(headingText);

        IText S(TextId textId) => tf.Center().NoWrap().Ink(Base.Ink.White).Format(textId);
        var options = new List<ContextMenuOption>
        {
            new(S( Base.SystemText.Gold_ThrowAway),
                new InventoryDiscardEvent(itemPosition.X, itemPosition.Y, _id.Id, _id.Slot),
                ContextMenuGroup.Actions)
        };

        var uiPosition = window.PixelToUi(cursorManager.Position);
        Raise(new ContextMenuEvent(uiPosition, heading, options));
    }
}

