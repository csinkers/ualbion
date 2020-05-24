using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Core.Events;
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
        readonly InventoryType _inventoryType;
        readonly int _id;
        readonly ItemSlotId _slotId;
        readonly VisualInventorySlot _visual;
        int _version;

        public LogicalInventorySlot(InventoryType inventoryType, int id, ItemSlotId slotId)
        {
            On<InventoryChangedEvent>(e =>
            {
                if (e.InventoryType == _inventoryType && e.InventoryId == _id)
                    _version++;
            });
            On<HoverEvent>(e =>
            {
                Hover();
                e.Propagating = false;
            });
            On<BlurEvent>(e =>
            {
                _visual.State = ButtonState.Normal;
                var inventoryManager = Resolve<IInventoryManager>();
                var hand = inventoryManager.ItemInHand;
                Raise(new SetCursorEvent(hand == null ? CoreSpriteId.Cursor : CoreSpriteId.CursorSmall));
                Raise(new HoverTextEvent(null));
            });

            _inventoryType = inventoryType;
            _id = id;
            _slotId = slotId;

            IText amountSource;
            if (slotId == ItemSlotId.Gold)
            {
                amountSource = new DynamicText(() =>
                {
                    var gold = Inventory.Gold;
                    return new[] { new TextBlock($"{gold / 10}.{gold % 10}") };
                }, x => _version);
            }
            else if (slotId == ItemSlotId.Rations)
            {
                amountSource = new DynamicText(() =>
                {
                    var food = Inventory.Rations;
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
            _visual = AttachChild(new VisualInventorySlot(slotId, amountSource, GetItem));
            _visual.Clicked += (sender, args) => OnClick();
            _visual.DoubleClicked += (sender, args) => OnDoubleClick();
            _visual.RightClicked += (sender, args) => OnRightClick();
        }

        public override string ToString() => $"InventorySlot:{_inventoryType}:{_id}:{_slotId}";

        IInventory Inventory => Resolve<IGameState>().GetInventory(_inventoryType, _id);
        ItemSlot Slot => Inventory.GetSlot(_slotId);
        ItemData GetItem()
        {
            var slot = Slot;
            return slot?.Id == null 
                ? null 
                : Resolve<IAssetManager>().LoadItem(slot.Id.Value);
        }

        void OnClick() => Raise(new InventoryPickupDropEvent(_inventoryType, _id, _slotId));
        void OnDoubleClick() => Raise(new InventoryPickupAllEvent(_inventoryType, _id, _slotId));

        void Hover()
        {
            var assets = Resolve<IAssetManager>();
            var settings = Resolve<ISettings>();
            var inventoryManager = Resolve<IInventoryManager>();
            var inventory = Resolve<IGameState>().GetInventory(_inventoryType, _id);
            var tf = Resolve<ITextFormatter>();

            var hand = inventoryManager.ItemInHand;
            if (hand is GoldInHand || hand is RationsInHand)
                return; // Don't show hover text when holding gold / food

            var slotInfo = inventory.GetSlot(_slotId);
            string itemName = null;
            if (slotInfo?.Id != null)
                itemName = assets.LoadString(slotInfo.Id.Value.ToId(), settings.Gameplay.Language);

            string itemInHandName = null;
            if (hand is ItemSlot handSlot && handSlot.Id.HasValue)
                itemInHandName = assets.LoadString(handSlot.Id.Value.ToId(), settings.Gameplay.Language);

            var action = inventoryManager.GetInventoryAction(_inventoryType, _id, _slotId);
            switch(action)
            {
                case InventoryAction.Pickup: // <Item name>
                {
                    if (itemName != null)
                    {
                        Raise(new HoverTextEvent(new LiteralText(itemName)));
                        Raise(new SetCursorEvent(CoreSpriteId.CursorSelected));
                        _visual.State = ButtonState.Hover;
                    }
                    break;
                }
                case InventoryAction.Drop: // Put down %s
                {
                    if (itemInHandName != null)
                    {
                        var text = tf.Format(SystemTextId.Item_PutDownX.ToId(), itemInHandName);
                        Raise(new HoverTextEvent(text));
                        _visual.State = ButtonState.Hover;
                    }
                    break;
                }
                case InventoryAction.Swap: // Swap %s with %s
                {
                    if (itemInHandName != null && itemName != null)
                    {
                        var text = tf.Format(SystemTextId.Item_SwapXWithX.ToId(), itemInHandName, itemName);
                        Raise(new HoverTextEvent(text));
                        _visual.State = ButtonState.Hover;
                    }
                    break;
                }
                case InventoryAction.Coalesce: // Add
                {
                    Raise(new HoverTextEvent(tf.Format(SystemTextId.Item_Add.ToId())));
                    _visual.State = ButtonState.Hover;
                    break;
                }
                case InventoryAction.NoCoalesceFullStack: // {YELLOW}This space is occupied!
                {
                    Raise(new HoverTextEvent(tf.Format(SystemTextId.Item_ThisSpaceIsOccupied.ToId())));
                    _visual.State = ButtonState.Hover;
                    break;
                }
            }
        }

        void OnRightClick()
        {
            var assets = Resolve<IAssetManager>();
            var window = Resolve<IWindowManager>();
            var cursorManager = Resolve<ICursorManager>();
            var inventory = Resolve<IGameState>().GetInventory(_inventoryType, _id);
            var tf = Resolve<ITextFormatter>();
            var slotInfo = inventory.GetSlot(_slotId);
            if (slotInfo?.Id == null) return;

            var item = assets.LoadItem(slotInfo.Id.Value);
            if (item == null)
                return;

            var heading = tf.Center().NoWrap().Fat().Format(item.Id.ToId());

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

            var options = new List<ContextMenuOption>
            {
                new ContextMenuOption(
                    S(SystemTextId.InvPopup_Drop.ToId(), (item.Flags & ItemFlags.PlotItem) != 0),
                    new InventoryDiscardEvent(_inventoryType, _id, _slotId),
                    ContextMenuGroup.Actions),

                new ContextMenuOption(
                    S(SystemTextId.InvPopup_Examine.ToId()),
                    new InventoryExamineEvent(item.Id),
                    ContextMenuGroup.Actions)
            };

            if(item.TypeId == ItemType.Document)
                options.Add(new ContextMenuOption( S(SystemTextId.InvPopup_Read.ToId()), null, ContextMenuGroup.Actions));

            if(item.TypeId == ItemType.SpellScroll)
                options.Add(new ContextMenuOption( S(SystemTextId.InvPopup_LearnSpell.ToId()), null, ContextMenuGroup.Actions));

            if(item.TypeId == ItemType.Drink)
                options.Add(new ContextMenuOption( S(SystemTextId.InvPopup_Drink.ToId()), null, ContextMenuGroup.Actions));

            if(item.TypeId == ItemType.HeadsUpDisplayItem)
                options.Add(new ContextMenuOption( S(SystemTextId.InvPopup_Activate.ToId()), null, ContextMenuGroup.Actions));

            if (item.Charges > 0) // TODO: Disable based on spell context
                options.Add(new ContextMenuOption(S(SystemTextId.InvPopup_ActivateSpell.ToId()), null, ContextMenuGroup.Actions));

            var inventoryManager = Resolve<IInventoryManager>();
            if (inventoryManager.ActiveMode == InventoryMode.Merchant)
            {
                options.Add(new ContextMenuOption(
                    S(SystemTextId.InvPopup_Sell.ToId(), (item.Flags & ItemFlags.PlotItem) != 0),
                    new InventorySellEvent(_inventoryType, _id, _slotId),
                    ContextMenuGroup.Actions));
            }

            var uiPosition = window.PixelToUi(cursorManager.Position);
            Raise(new ContextMenuEvent(uiPosition, heading, options));
        }
    }
}
