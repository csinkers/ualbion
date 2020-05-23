using System.Collections.Generic;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Assets;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Gui.Text;
using UAlbion.Game.Input;
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

        public InventorySlot(InventoryType inventoryType, int id, ItemSlotId slotId)
        {
            On<UiLeftClickEvent>(e => OnClick());
            On<UiRightClickEvent>(OnRightClick);
            On<IdleClockEvent>(e => _frameNumber++);
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
                _frame.State = ButtonState.Normal;
                var inventoryManager = Resolve<IInventoryManager>();
                var hand = inventoryManager.ItemInHand;
                Raise(new SetCursorEvent(hand == null ? CoreSpriteId.Cursor : CoreSpriteId.CursorSmall));
                Raise(new HoverTextEvent(""));
            });
            On<TimerElapsedEvent>(e =>
            {
                if (e.Id == TimerName)
                    OnTimer();
            });

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
                Theme = slotId.IsBodyPart() ? (ButtonFrame.ThemeFunction)ButtonTheme.Default : ButtonTheme.InventorySlot,
                State = slotId.IsBodyPart() ? ButtonState.Normal : ButtonState.Pressed
            });
        }

        public override string ToString() => $"InventorySlot:{_inventoryType}:{_id}:{_slotId}";

        void GetSlot(out ItemSlot slotInfo, out ItemData item)
        {
            var assets = Resolve<IAssetManager>();
            var inventory = Resolve<IGameState>().GetInventory(_inventoryType, _id);

            slotInfo = inventory.GetSlot(_slotId);
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
            var inventoryManager = Resolve<IInventoryManager>();
            var inventory = Resolve<IGameState>().GetInventory(_inventoryType, _id);

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
                case InventoryAction.Pickup:
                {
                    // <Item name>
                    if (itemName != null)
                    {
                        Raise(new HoverTextEvent(itemName));
                        Raise(new SetCursorEvent(CoreSpriteId.CursorSelected));
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
                        Raise(new HoverTextExEvent(BuildHoverText(
                            SystemTextId.Item_SwapXWithX,
                            itemInHandName,
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

        void OnRightClick(UiRightClickEvent e)
        {
            e.Propagating = false;

            var assets = Resolve<IAssetManager>();
            var window = Resolve<IWindowManager>();
            var settings = Resolve<ISettings>();
            var cursorManager = Resolve<ICursorManager>();
            var inventory = Resolve<IGameState>().GetInventory(_inventoryType, _id);
            var slotInfo = inventory.GetSlot(_slotId);
            if (slotInfo?.Id == null) return;

            var item = assets.LoadItem(slotInfo.Id.Value);
            if (item == null)
                return;

            var heading = assets.FormatText(item.Id.ToId(), settings.Gameplay.Language, f => { f.Centre().NoWrap().Fat(); });

            IText S(StringId textId, bool disabled = false) => assets.FormatText(textId, settings.Gameplay.Language, f =>
                    {
                        f.Centre().NoWrap();
                        if (disabled)
                            f.Ink(FontColor.Yellow);
                    });

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

            /*
            options.Add(new ContextMenuOption( S(SystemTextId.InvPopup_Sell), null, ContextMenuGroup.Actions));
            options.Add(new ContextMenuOption( S(SystemTextId.InvPopup_Use), null, ContextMenuGroup.Actions));

            IText SL(string text) => new LiteralText(text);
            if ((item.Flags & ItemFlags.Unk0)  != 0) options.Add(new ContextMenuOption(SL("0"), null, ContextMenuGroup.Actions2));
            if ((item.Flags & ItemFlags.Unk2)  != 0) options.Add(new ContextMenuOption(SL("2"), null, ContextMenuGroup.Actions2));
            if ((item.Flags & ItemFlags.Unk3)  != 0) options.Add(new ContextMenuOption(SL("3"), null, ContextMenuGroup.Actions2));
            if ((item.Flags & ItemFlags.Unk4)  != 0) options.Add(new ContextMenuOption(SL("4"), null, ContextMenuGroup.Actions2));
            if ((item.Flags & ItemFlags.Unk5)  != 0) options.Add(new ContextMenuOption(SL("5"), null, ContextMenuGroup.Actions2));
            if ((item.Flags & ItemFlags.Unk6)  != 0) options.Add(new ContextMenuOption(SL("6"), null, ContextMenuGroup.Actions2));
            if ((item.Flags & ItemFlags.Unk7)  != 0) options.Add(new ContextMenuOption(SL("7"), null, ContextMenuGroup.Actions2));
            if ((item.Flags & ItemFlags.Unk8)  != 0) options.Add(new ContextMenuOption(SL("8"), null, ContextMenuGroup.Actions2));
            if ((item.Flags & ItemFlags.Unk9)  != 0) options.Add(new ContextMenuOption(SL("9"), null, ContextMenuGroup.Actions2));
            if ((item.Flags & ItemFlags.Unk11) != 0) options.Add(new ContextMenuOption(SL("11"), null, ContextMenuGroup.Actions2));
            if ((item.Flags & ItemFlags.Unk12) != 0) options.Add(new ContextMenuOption(SL("12"), null, ContextMenuGroup.Actions2));
            if ((item.Flags & ItemFlags.TailWieldable) != 0) options.Add(new ContextMenuOption(SL("13 TailWieldable"), null, ContextMenuGroup.Actions2));
            if ((item.Flags & ItemFlags.Stackable) != 0) options.Add(new ContextMenuOption(SL("14 Stackable"), null, ContextMenuGroup.Actions2));
            if ((item.Flags & ItemFlags.TwoHanded) != 0) options.Add(new ContextMenuOption(SL("15 TwoHanded"), null, ContextMenuGroup.Actions2));
            */

            var uiPosition = window.PixelToUi(cursorManager.Position);
            Raise(new ContextMenuEvent(uiPosition, heading, options));
        }
    }
}
