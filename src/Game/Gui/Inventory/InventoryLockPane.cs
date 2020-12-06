using System;
using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Formats.Assets;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;
using UAlbion.Game.Input;
using UAlbion.Game.State;
using UAlbion.Game.State.Player;
using UAlbion.Game.Text;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryLockPane : UiElement
    {
        readonly ILockedInventoryEvent _lockEvent;

        public InventoryLockPane(ILockedInventoryEvent lockEvent)
        {
            On<PickLockEvent>(_ => PickLock());
            _lockEvent = lockEvent;
            bool isChest = lockEvent is ChestEvent;
            var background = new UiSpriteElement(isChest ? Base.Picture.ClosedChest : Base.Picture.WoodenDoor);
            var backgroundStack = new FixedPositionStack();
            backgroundStack.Add(background, 0, 0);
            AttachChild(backgroundStack);

            var lockButton = 
                new Button(
                    new Padding(
                        new UiSpriteElement(Base.CoreSprite.Lock),
                        4))
                .OnHover(LockHovered)
                .OnBlur(() =>
                {
                    Raise(new HoverTextEvent(null));
                    if (Resolve<IInventoryManager>().ItemInHand.Item == null)
                        Raise(new SetCursorEvent(Base.CoreSprite.Cursor));
                })
                .OnClick(LockClicked) // If holding key etc
                .OnRightClick(LockRightClicked)
                ;
            AttachChild(new FixedPosition(new Rectangle(50, isChest ? 50 : 112, 32, 32), lockButton));
        }

        void LockHovered()
        {
            var tf = Resolve<ITextFormatter>();
            Raise(new HoverTextEvent(tf.Format(Base.SystemText.Lock_OpenTheLock)));
            if (Resolve<IInventoryManager>().ItemInHand.Item == null)
                Raise(new SetCursorEvent(Base.CoreSprite.CursorSelected));
        }

        void LockClicked()
        {
            var hand = Resolve<IInventoryManager>().ItemInHand;
            if (hand.ItemId.IsNone)
                return;

            var tf = Resolve<ITextFormatter>();
            if (hand.ItemId == _lockEvent.KeyItemId)
            {
                Raise(new HoverTextEvent(tf.Format(Base.SystemText.Lock_LeaderOpenedTheLock)));
                Raise(new InventoryReturnItemInHandEvent());
                Raise(new LockOpenedEvent());
            }
            else if (hand.ItemId == Base.Item.Lockpick)
            {
                if (_lockEvent.PickDifficulty == 100)
                {
                    Raise(new DescriptionTextEvent(tf.Format(Base.SystemText.Lock_ThisLockCannotBePicked)));
                    Raise(new InventoryDestroyItemInHandEvent());
                }
                else
                {
                    Raise(new InventoryDestroyItemInHandEvent());
                    Raise(new DescriptionTextEvent(tf.Format(Base.SystemText.Lock_LeaderPickedTheLockWithALockpick)));
                    Raise(new LockOpenedEvent());
                }
            }
            else if (hand.Item is ItemData item)
            {
                Raise(new DescriptionTextEvent(tf.Format(
                    item.TypeId == ItemType.Key 
                    ? Base.SystemText.Lock_ThisIsNotTheRightKey 
                    : Base.SystemText.Lock_YouCannotOpenTheLockWithThisItem)));
            }
        }

        void CanPick(IPlayer player, Action<bool> continuation)
        {
            var skill = player.Effective.Skills.LockPicking;
            if (skill == 0)
            {
                continuation(false);
                return;
            }

            // TODO: Determine the actual probabilities the game uses.
            var baseChance = (100.0f - _lockEvent.PickDifficulty) / 100.0f;
            var adjusted = baseChance * skill;
            RaiseAsync(QueryEvent.RandomChance((ushort)adjusted), continuation);
        }

        void PickLock()
        {
            var tf = Resolve<ITextFormatter>();
            if (_lockEvent.PickDifficulty >= 100)
            {
                Raise(new DescriptionTextEvent(tf.Format(Base.SystemText.Lock_ThisLockCannotBePicked)));
                return;
            }

            var leader = Resolve<IParty>().Leader;
            CanPick(leader, x =>
            {
                if (x)
                {
                    Raise(new DescriptionTextEvent(tf.Format(Base.SystemText.Lock_LeaderPickedTheLock)));
                    Raise(new LockOpenedEvent());
                }
                else
                {
                    Raise(new DescriptionTextEvent(tf.Format(Base.SystemText.Lock_LeaderCannotPickThisLock)));
                }
            });
        }

        void LockRightClicked()
        {
            // ContextMenu: Lock, Pick the lock
            var options = new List<ContextMenuOption>();
            var tf = Resolve<ITextFormatter>();
            var window = Resolve<IWindowManager>();
            var cursorManager = Resolve<ICursorManager>();

            options.Add(new ContextMenuOption(
                tf.Center().NoWrap().Format(Base.SystemText.Lock_PickTheLock),
                new PickLockEvent(),
                ContextMenuGroup.Actions));

            var heading = tf.Center().NoWrap().Fat().Format(Base.SystemText.Lock_Lock);
            var uiPosition = window.PixelToUi(cursorManager.Position);
            Raise(new ContextMenuEvent(uiPosition, heading, options));
        }
    }
}
