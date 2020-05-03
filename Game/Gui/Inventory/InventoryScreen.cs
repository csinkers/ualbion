using System;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using UAlbion.Game.Events.Inventory;
using UAlbion.Game.Gui.Controls;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryScreen : Dialog
    {
        const string ExitButtonId = "Inventory.Exit";
        readonly InventoryConfig _config;
        InventoryMode _mode;
        int _modeSpecificId;
        InventoryPage _page;
        PartyCharacterId _activeCharacter;

        public InventoryScreen(InventoryConfig config) : base(DialogPositioning.Bottom)
        {
            On<InventoryModeEvent>(SetMode);
            On<InventoryChestModeEvent>(SetMode);
            On<InventoryMerchantModeEvent>(SetMode);
            On<ButtonPressEvent>(e => OnButton(e.ButtonId));

            _config = config;
        }

        protected override void Subscribed()
        {
            Rebuild();
        }

        void SetMode(ISetInventoryModeEvent e)
        {
            _mode = e.Mode;
            _modeSpecificId = e switch
            {
                InventoryChestModeEvent chest => (int)chest.ChestId,
                InventoryMerchantModeEvent merchant => (int)merchant.MerchantId,
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

                    // InventoryMode.Merchant => new InventoryChestPane(false, _modeSpecificId), // TODO
                    InventoryMode.Chest => new InventoryChestPane((ChestId)_modeSpecificId),
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
