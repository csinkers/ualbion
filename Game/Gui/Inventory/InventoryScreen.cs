using System;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryScreen : Dialog
    {
        const string ExitButtonId = "Inventory.Exit";
        static readonly HandlerSet Handlers = new HandlerSet(
            H<InventoryScreen, SetInventoryModeEvent>((x,e) => x.SetMode(e)),
            H<InventoryScreen, SetInventoryChestModeEvent>((x,e) => x.SetMode(e)),
            H<InventoryScreen, SetInventoryMerchantModeEvent>((x,e) => x.SetMode(e)),
            H<InventoryScreen, ButtonPressEvent>((x, e) => x.OnButton(e.ButtonId))
        );

        readonly InventoryConfig _config;
        InventoryMode _mode;
        int _modeSpecificId;
        InventoryPage _page;
        PartyCharacterId _activeCharacter;

        public InventoryScreen(InventoryConfig config) : base(Handlers, DialogPositioning.Bottom)
        {
            _config = config;
        }

        public override void Subscribed()
        {
            Rebuild();
            base.Subscribed();
        }

        void SetMode(ISetInventoryModeEvent e)
        {
            _mode = e.Mode;
            _modeSpecificId = e switch
            {
                SetInventoryChestModeEvent chest => (int)chest.ChestId,
                SetInventoryMerchantModeEvent merchant => (int)merchant.MerchantId,
                // SetInventoryDoorModeEvent door => (int)door.DoorId
                _ => 0
            };
            // TODO: Verify that the party member is currently in the party
            _activeCharacter = e.Member;
            Rebuild();
        }

        void OnButton(string buttonId)
        {
            switch (buttonId)
            {
                case ExitButtonId: Raise(new PopSceneEvent()); break;
                default: return;
            }
        }

        void Rebuild()
        {
            foreach(var child in Children)
                child.Detach();
            Children.Clear();

            var leftPane =
                _mode switch
                {
                    InventoryMode.Character => (IUiElement)new InventoryCharacterPane(
                        _activeCharacter,
                        () => _page,
                        x => _page = x),

                    InventoryMode.Merchant => new InventoryChestPane(false, _modeSpecificId),
                    InventoryMode.Chest => new InventoryChestPane(true, _modeSpecificId),
                    InventoryMode.LockedChest => new InventoryLockPane(true),
                    InventoryMode.LockedDoor => new InventoryLockPane(false),
                    _ => throw new InvalidOperationException($"Unexpected inventory mode {_mode}")
                };

            var middlePane = new InventoryMidPane(_activeCharacter, _config.Positions[_activeCharacter]);
            var rightPane = new InventoryRightPane(_activeCharacter, ExitButtonId, _mode == InventoryMode.Merchant);
            // var frameDivider = new FrameDivider(135, 0, 4, 192);

            AttachChild(new UiFixedPositionElement<SlabId>(SlabId.SLAB, UiConstants.UiExtents));
            AttachChild(new FixedPosition(new Rectangle(0, 0, 135, UiConstants.ActiveAreaExtents.Height), leftPane));
            AttachChild(new FixedPosition(new Rectangle(142, 0, 134, UiConstants.ActiveAreaExtents.Height), middlePane));
            AttachChild(new FixedPosition(new Rectangle(280, 0, 71, UiConstants.ActiveAreaExtents.Height), rightPane));
        }
    }
}
