using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.State;
using UAlbion.Game.State.Player;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventorySlot : UiElement
    {
        const string TimerName = "InventorySlot.ClickTimer";

        readonly InventoryType _inventoryType;
        readonly int _id;
        readonly ItemSlotId _slotId;
        readonly ButtonFrame _frame;
        readonly UiSpriteElement<ItemSpriteId> _sprite;
        readonly Vector2 _size;

        int _version;
        int _frameNumber;
        bool _isClickTimerPending;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<InventorySlot, InventoryChangedEvent>((x, e) =>
            {
                if (e.InventoryType == x._inventoryType && e.InventoryId == x._id)
                    x._version++;
            }),
            H<InventorySlot, SlowClockEvent>((x, e) => x._frameNumber += e.Delta),
            H<InventorySlot, HoverEvent>((x, e) =>
            {
                x.Hover();
                e.Propagating = false;
            }),
            H<InventorySlot, BlurEvent>((x, _) =>
            {
                x._frame.State = ButtonState.Normal;
                x.Raise(new HoverTextEvent(""));
            }),
            H<InventorySlot, UiLeftClickEvent>((x, _) => x.OnClick()),
            H<InventorySlot, TimerElapsedEvent>((x, e) =>
            {
                if (e.Id == TimerName) x.OnTimer();
            })
        );

        public InventorySlot(InventoryType inventoryType, int id, ItemSlotId slotId) : base(Handlers)
        {
            _inventoryType = inventoryType;
            _id = id;
            _slotId = slotId;
            _size = slotId.IsBodyPart() ? new Vector2(16, 16) : new Vector2(16, 20);
            _sprite = new UiSpriteElement<ItemSpriteId>(0) { SubId = (int)ItemSpriteId.Nothing };

            var amountSource = new DynamicText(() =>
            {
                GetSlot(out var slotInfo, out _);
                return slotInfo == null || slotInfo.Amount < 2
                    ? new TextBlock[0]
                    : new[] { new TextBlock(slotInfo.Amount.ToString()) { Alignment = TextAlignment.Right } };
            }, x => _version);

            var text = new TextElement(amountSource);

            _frame = AttachChild(new ButtonFrame(new FixedPositionStack()
                .Add(_sprite, 0, 0, 16, 16)
                .Add(text, 0, 20 - 9, 16, 9))
            {
                Padding = -1,
                Theme = slotId.IsBodyPart() ? (ButtonFrame.ITheme)new ButtonTheme() : new InventorySlotTheme(),
                State = slotId.IsBodyPart() ? ButtonState.Normal : ButtonState.Pressed
            });
        }

        public override string ToString() => $"InventorySlot:{_inventoryType}:{_id}:{_slotId}";

        IInventory Inventory
        {
            get
            {
                var state = Resolve<IGameState>();
                return _inventoryType switch
                {
                    InventoryType.Player => state.GetPartyMember((PartyCharacterId)_id)?.Inventory,
                    InventoryType.Chest => state.GetChest((ChestId)_id),
                    InventoryType.Merchant => state.GetMerchant((MerchantId)_id),
                    _ => throw new InvalidOperationException($"Invalid inventory type \"{_inventoryType}\"")
                };
            }
        }

        void GetSlot(out ItemSlot slotInfo, out ItemData item)
        {
            var assets = Resolve<IAssetManager>();
            slotInfo = Inventory.GetSlot(_slotId);
            item = slotInfo?.Id == null ? null : assets.LoadItem(slotInfo.Id.Value);
        }

        void OnClick()
        {
            if (_isClickTimerPending) // If they double-clicked...
            {
                Raise(new InventoryPickupDropItemEvent(_inventoryType, _id, _slotId));
                _isClickTimerPending = false; // Ensure the single-click behaviour doesn't happen.
            }
            else // For the first click, just start the double-click timer.
            {
                // TODO: If single item, fire the pickup event immediately
                Raise(new StartTimerEvent(TimerName, 300, this));
                _isClickTimerPending = true;
            }
        }

        void OnTimer()
        {
            if (!_isClickTimerPending) // They've already double-clicked
                return;

            // TODO: Show quantity selection dialog
            Raise(new InventoryPickupDropItemEvent(_inventoryType, _id, _slotId, 1));
            _isClickTimerPending = false;
        }

        DynamicText BuildHoverText(SystemTextId template, params object[] arguments) =>
            new DynamicText(() =>
            {
                var assets = Resolve<IAssetManager>();
                var settings = Resolve<ISettings>();
                var textFormatter = new TextFormatter(assets, settings.Gameplay.Language);
                return textFormatter.Format(template, arguments).Blocks;
            });

        void Hover()
        {
            var assets = Resolve<IAssetManager>();
            var settings = Resolve<ISettings>();
            var inventory = Resolve<IInventoryManager>();

            var hand = inventory.ItemInHand;
            if (hand is GoldInHand || hand is RationsInHand)
                return; // Don't show hover text when holding gold / food

            var slotInfo = Inventory.GetSlot(_slotId);
            string itemName = null;
            if (slotInfo?.Id != null)
            {
                var item = assets.LoadItem(slotInfo.Id.Value);
                if (item != null)
                    itemName = item.GetName(settings.Gameplay.Language);
            }

            string itemInHandName = null;
            if (hand is ItemSlot handSlot && handSlot.Id.HasValue)
            {
                var itemInHand = assets.LoadItem(handSlot.Id.Value);
                itemInHandName = itemInHand.GetName(settings.Gameplay.Language);
            }

            var inventoryManager = Resolve<IInventoryManager>();
            var action = inventoryManager.GetInventoryAction(_inventoryType, _id, _slotId);
            switch(action)
            {
                case InventoryAction.Pickup:
                {
                    // <Item name>
                    if (itemName != null)
                    {
                        Raise(new HoverTextEvent(itemName));
                        _frame.State = ButtonState.Hover;
                    }
                    break;
                }
                case InventoryAction.Drop:
                {
                    if (itemInHandName != null)
                    {
                        // Put down %s
                        Raise(new HoverTextExEvent(BuildHoverText(SystemTextId.Item_PutDownX, itemInHandName)));
                        _frame.State = ButtonState.Hover;
                    }

                    break;
                }
                case InventoryAction.Swap:
                {
                    if (itemInHandName != null && itemName != null)
                    {
                        // Swap %s with %s
                        Raise(new HoverTextExEvent(BuildHoverText(SystemTextId.Item_SwapXWithX, itemInHandName,
                            itemName)));
                        _frame.State = ButtonState.Hover;
                    }

                    break;
                }
                case InventoryAction.Coalesce:
                {
                    // Add
                    Raise(new HoverTextExEvent(BuildHoverText(SystemTextId.Item_Add)));
                    _frame.State = ButtonState.Hover;
                    break;
                }
                case InventoryAction.NoCoalesceFullStack:
                {
                    // {YELLOW}This space is occupied!
                    Raise(new HoverTextExEvent(BuildHoverText(SystemTextId.Item_ThisSpaceIsOccupied)));
                    _frame.State = ButtonState.Hover;
                    break;
                }
            }

        }

        void Rebuild()
        {
            GetSlot(out _, out var item);

            if(item == null)
            {
                _sprite.SubId = (int)ItemSpriteId.Nothing;
                return;
            }

            int frames = item.IconAnim == 0 ? 1 : item.IconAnim;
            while (_frameNumber >= frames)
                _frameNumber -= frames;

            int itemSpriteId = (int)item.Icon + _frameNumber;
            _sprite.SubId = itemSpriteId;
            // TODO: Show item.Amount
            // TODO: Show broken overlay if item.Flags.HasFlag(ItemSlotFlags.Broken)
        }

        public override int Render(Rectangle extents, int order)
        {
            Rebuild();
            return base.Render(extents, order);
        }

        public override Vector2 GetSize() => _size;
    }
}
