using System.Collections.Generic;
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
                    var gold = Inventory.Gold.Amount;
                    return new[] { new TextBlock($"{gold / 10}.{gold % 10}") };
                }, x => _version);
            }
            else if (id.Slot == ItemSlotId.Rations)
            {
                amountSource = new DynamicText(() =>
                {
                    var food = Inventory.Rations.Amount;
                    return new[] { new TextBlock(food.ToString()) };
                }, x => _version);
            }
            else
            {
                amountSource = new DynamicText(() =>
                {
                    var slotInfo = Slot;
                    return slotInfo == null || slotInfo.Amount < 2
                        ? new TextBlock[0]
                        : new[] { new TextBlock(slotInfo.Amount.ToString()) { Alignment = TextAlignment.Right } };
                }, x => _version);
            }

            _visual = AttachChild(new VisualInventorySlot(_id, amountSource, () => Slot))
                .OnClick(() => Raise(new InventorySwapEvent(_id.Type, _id.Id, _id.Slot)))
                .OnDoubleClick(() => Raise(new InventoryPickupAllEvent(_id.Type, _id.Id, _id.Slot)))
                .OnRightClick(OnRightClick)
                .OnHover(Hover)
                .OnBlur(Blur);
        }

        public override string ToString() => $"InventorySlot:{_id}";
        IInventory Inventory => Resolve<IGameState>().GetInventory(_id.Inventory);
        IReadOnlyItemSlot Slot => Inventory.GetSlot(_id.Slot);

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

            var slotInfo = inventory.GetSlot(_id.Slot);
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
            var options = new List<ContextMenuOption>
            {
                new ContextMenuOption(
                    S(SystemTextId.InvPopup_Drop, isPlotItem),
                    isPlotItem
                        ? (IEvent)new HoverTextEvent(
                            tf.Format(
                                SystemTextId.InvMsg_ThisIsAVitalItem))
                        : new InventoryDiscardEvent(itemPosition.X, itemPosition.Y,_id.Type, _id.Id, _id.Slot),
                    ContextMenuGroup.Actions,
                    isPlotItem),

                new ContextMenuOption(
                    S(SystemTextId.InvPopup_Examine),
                    new InventoryExamineEvent(item.Id),
                    ContextMenuGroup.Actions)
            };

            if (item.TypeId == ItemType.Document)
                options.Add(new ContextMenuOption(S(SystemTextId.InvPopup_Read), null, ContextMenuGroup.Actions));

            if (item.TypeId == ItemType.SpellScroll)
                options.Add(new ContextMenuOption(S(SystemTextId.InvPopup_LearnSpell), null, ContextMenuGroup.Actions));

            if (item.TypeId == ItemType.Drink)
                options.Add(new ContextMenuOption(S(SystemTextId.InvPopup_Drink), null, ContextMenuGroup.Actions));

            if (item.TypeId == ItemType.HeadsUpDisplayItem)
                options.Add(new ContextMenuOption(S(SystemTextId.InvPopup_Activate), null, ContextMenuGroup.Actions));

            if (item.Charges > 0) // TODO: Disable based on spell context
                options.Add(new ContextMenuOption(S(SystemTextId.InvPopup_ActivateSpell), null, ContextMenuGroup.Actions));

            var inventoryManager = Resolve<IInventoryManager>();
            if (inventoryManager.ActiveMode == InventoryMode.Merchant)
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

            var uiPosition = window.PixelToUi(cursorManager.Position);
            Raise(new ContextMenuEvent(uiPosition, heading, options));
        }
    }
}
