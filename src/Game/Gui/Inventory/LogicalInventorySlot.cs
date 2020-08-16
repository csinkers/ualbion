using System;
using System.Collections.Generic;
using System.Globalization;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Input;
using UAlbion.Game.State;
using UAlbion.Game.State.Player;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory
{
    public class LogicalInventorySlot : UiElement
    {
        readonly InventorySlotId _id;
        readonly VisualInventorySlot _visual;
        int _version;

        public LogicalInventorySlot(InventorySlotId id)
        {
            On<InventoryChangedEvent>(e =>
            {
                if (e.InventoryType == _id.Type && e.InventoryId == _id.Id)
                    _version++;
            });

            _id = id;

            IText amountSource;
            if (id.Slot == ItemSlotId.Gold)
            {
                amountSource = new DynamicText(() =>
                {
                    var gold = Inventory?.Gold.Amount ?? 0;
                    return new[] { new TextBlock($"{gold / 10}.{gold % 10}") }; // i18n: May need to vary based on the current game language
                }, x => _version);
            }
            else if (id.Slot == ItemSlotId.Rations)
            {
                amountSource = new DynamicText(() =>
                {
                    var food = Inventory?.Rations.Amount ?? 0;
                    return new[] { new TextBlock(food.ToString(CultureInfo.InvariantCulture)) }; // i18n: Will need to be changed if we support a language that doesn't use Hindu-Arabic numerals.
                }, x => _version);
            }
            else
            {
                amountSource = new DynamicText(() =>
                {
                    var slotInfo = Slot;
                    return slotInfo == null || slotInfo.Amount < 2
                        ? Array.Empty<TextBlock>()
                        : new[] { new TextBlock(slotInfo.Amount.ToString(CultureInfo.InvariantCulture)) { Alignment = TextAlignment.Right } }; // i18n: Will need to be changed if we support a language that doesn't use Hindu-Arabic numerals.
                }, x => _version);
            }

            _visual = AttachChild(new VisualInventorySlot(_id, amountSource, () => Slot))
                .OnButtonDown(() =>
                {
                    var im = Resolve<IInventoryManager>();
                    var inputBinder = Resolve<IInputBinder>();
                    if (im.ItemInHand.Item != null
                        || inputBinder.IsCtrlPressed
                        || inputBinder.IsShiftPressed
                        || inputBinder.IsAltPressed)
                    {
                        _visual.SuppressNextDoubleClick = true;
                    }
                })
                .OnClick(() =>
                {
                    var inputBinder = Resolve<IInputBinder>();
                    if (inputBinder.IsCtrlPressed)
                        Raise(new InventoryPickupEvent(null, _id.Type, _id.Id, _id.Slot));
                    else if (inputBinder.IsShiftPressed)
                        Raise(new InventoryPickupEvent(5, _id.Type, _id.Id, _id.Slot));
                    else if (inputBinder.IsAltPressed)
                        Raise(new InventoryPickupEvent(1, _id.Type, _id.Id, _id.Slot));
                    else
                        Raise(new InventorySwapEvent(_id.Type, _id.Id, _id.Slot));
                })
                .OnDoubleClick(() => Raise(new InventoryPickupEvent(null, _id.Type, _id.Id, _id.Slot)))
                .OnRightClick(OnRightClick)
                .OnHover(Hover)
                .OnBlur(Blur);
        }

        public override string ToString() => $"InventorySlot:{_id}";
        IInventory Inventory => Resolve<IGameState>().GetInventory(_id.Inventory);
        IReadOnlyItemSlot Slot => Inventory?.GetSlot(_id.Slot);

        void Blur()
        {
            var inventoryManager = Resolve<IInventoryManager>();
            var hand = inventoryManager.ItemInHand;
            Raise(new SetCursorEvent(hand.Item == null ? CoreSpriteId.Cursor : CoreSpriteId.CursorSmall));
            Raise(new HoverTextEvent(null));
        }

        void Hover()
        {
            var assets = Resolve<IAssetManager>();
            var settings = Resolve<ISettings>();
            var inventoryManager = Resolve<IInventoryManager>();
            var inventory = Resolve<IGameState>().GetInventory(_id.Inventory);
            var tf = Resolve<ITextFormatter>();

            var slotInfo = Slot;
            string itemName = null;
            if (slotInfo?.Item is ItemData item)
                itemName = assets.LoadString(item.Id, settings.Gameplay.Language);

            var hand = inventoryManager.ItemInHand;
            string itemInHandName = null;
            if (hand.Item is ItemData itemInHand)
                itemInHandName = assets.LoadString(itemInHand.Id, settings.Gameplay.Language);

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
                        Raise(new SetCursorEvent(CoreSpriteId.CursorSelected));
                    }
                    else if(_id.Slot == ItemSlotId.Gold || _id.Slot == ItemSlotId.Rations)
                    {
                        bool isGold = _id.Slot == ItemSlotId.Gold;
                        int amount = isGold ? inventory.Gold.Amount : inventory.Rations.Amount;
                        var text = isGold
                            ? tf.Format(SystemTextId.Gold_NNGold, amount / 10, amount % 10)
                            : tf.Format(SystemTextId.Gold_NRations, amount);
                        Raise(new HoverTextEvent(text));
                        Raise(new SetCursorEvent(CoreSpriteId.CursorSelected));
                    }
                    break;
                }
                case InventoryAction.PutDown: // Put down %s
                {
                    if (itemInHandName != null)
                    {
                        var text = tf.Format(SystemTextId.Item_PutDownX, itemInHandName);
                        Raise(new HoverTextEvent(text));
                    }
                    break;
                }
                case InventoryAction.Swap: // Swap %s with %s
                {
                    if (itemInHandName != null && itemName != null)
                    {
                        var text = tf.Format(SystemTextId.Item_SwapXWithX, itemInHandName, itemName);
                        Raise(new HoverTextEvent(text));
                    }
                    break;
                }
                case InventoryAction.Coalesce: // Add
                {
                    Raise(new HoverTextEvent(tf.Format(SystemTextId.Item_Add)));
                    break;
                }
                case InventoryAction.NoCoalesceFullStack: // {YELLOW}This space is occupied!
                {
                    Raise(new HoverTextEvent(tf.Format(SystemTextId.Item_ThisSpaceIsOccupied)));
                    break;
                }
            }
        }

        void OnRightClick()
        {
            var window = Resolve<IWindowManager>();
            var cursorManager = Resolve<ICursorManager>();
            var inventory = Resolve<IGameState>().GetInventory(_id.Inventory);
            var tf = Resolve<ITextFormatter>();
            var slotInfo = inventory.GetSlot(_id.Slot);
            if (!(slotInfo?.Item is ItemData item))
                return;

            var itemPosition = window.UiToNorm(slotInfo.LastUiPosition);
            var heading = tf.Center().NoWrap().Fat().Format(item.Id);

            IText S(StringId textId, bool disabled = false)
                => tf
                .Center()
                .NoWrap()
                .Ink(disabled ? FontColor.Yellow : FontColor.White)
                .Format(textId);

            // Drop (Yellow inactive when critical)
            // Examine
            // Use (e.g. torch)
            // Drink
            // Activate (compass, clock, monster eye)
            // Activate spell (if has spell, yellow if combat spell & not in combat etc)
            // Read (e.g. metalmagic knowledge, maps)

            bool isPlotItem = (item.Flags & ItemFlags.PlotItem) != 0;
            var options = new List<ContextMenuOption>();

            if (_id.Type == InventoryType.Merchant)
            {
                options.Add(new ContextMenuOption(
                    S(SystemTextId.InvPopup_Sell, isPlotItem),
                    isPlotItem 
                        ? (IEvent)new HoverTextEvent(
                            tf.Format(
                                SystemTextId.InvMsg_ThisIsAVitalItem))
                        : new InventorySellEvent(_id.Type, _id.Id, _id.Slot),
                    ContextMenuGroup.Actions,
                    isPlotItem));
            }
            else
            {
                options.Add(
                    new ContextMenuOption(
                        S(SystemTextId.InvPopup_Drop, isPlotItem),
                        isPlotItem
                            ? (IEvent)new HoverTextEvent(
                                tf.Format(
                                    SystemTextId.InvMsg_ThisIsAVitalItem))
                            : new InventoryDiscardEvent(itemPosition.X, itemPosition.Y, _id.Type, _id.Id, _id.Slot),
                        ContextMenuGroup.Actions,
                        isPlotItem));
            }

            options.Add(new ContextMenuOption(
                S(SystemTextId.InvPopup_Examine),
                new InventoryExamineEvent(item.Id),
                ContextMenuGroup.Actions));

            if (item.TypeId == ItemType.Document && _id.Type == InventoryType.Player)
                options.Add(new ContextMenuOption(S(SystemTextId.InvPopup_Read), null, ContextMenuGroup.Actions));

            if (item.TypeId == ItemType.SpellScroll && _id.Type == InventoryType.Player)
                options.Add(new ContextMenuOption(S(SystemTextId.InvPopup_LearnSpell), null, ContextMenuGroup.Actions));

            if (item.TypeId == ItemType.Drink && _id.Type == InventoryType.Player)
                options.Add(new ContextMenuOption(S(SystemTextId.InvPopup_Drink), null, ContextMenuGroup.Actions));

            if (item.TypeId == ItemType.HeadsUpDisplayItem && _id.Type == InventoryType.Player)
                options.Add(new ContextMenuOption(S(SystemTextId.InvPopup_Activate), null, ContextMenuGroup.Actions));

            if (item.Charges > 0 && _id.Type == InventoryType.Player) // TODO: Disable based on spell context
                options.Add(new ContextMenuOption(S(SystemTextId.InvPopup_ActivateSpell), null, ContextMenuGroup.Actions));


            var uiPosition = window.PixelToUi(cursorManager.Position);
            Raise(new ContextMenuEvent(uiPosition, heading, options));
        }
    }
}
