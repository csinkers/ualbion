using System;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryCharacterPane : UiElement
    {
        readonly Func<InventoryPage> _getPage;
        public const string SummaryButtonId = "Inventory.SummaryPage";
        public const string StatsButtonId = "Inventory.StatsPage";
        public const string MiscButtonId = "Inventory.MiscPage";

        readonly Button _summaryButton;
        readonly Button _statsButton;
        readonly Button _miscButton;

        public InventoryCharacterPane(PartyCharacterId activeCharacter, Func<InventoryPage> getPage)
        {
            _getPage = getPage;
            var page = _getPage();
            _summaryButton = new Button(SummaryButtonId, "I") { DoubleFrame = true, IsPressed = page == InventoryPage.Summary };
            _statsButton = new Button(StatsButtonId, "II") { DoubleFrame = true, IsPressed = page == InventoryPage.Stats };
            _miscButton = new Button(MiscButtonId, "III") { DoubleFrame = true, IsPressed = page == InventoryPage.Misc };
            var buttonStack =
                new FixedPosition(
                    new Rectangle(84, 174, 50, 15),
                    new HorizontalStack(
                    new FixedSize(16, 15, _summaryButton),
                    new FixedSize(16, 15, _statsButton),
                    new FixedSize(16, 15, _miscButton)
                ));

            AttachChild(buttonStack);
            AttachChild(new InventoryActivePageSelector(activeCharacter, getPage));
        }

        public override int Render(Rectangle extents, int order)
        {
            var page = _getPage();
            _summaryButton.IsPressed = page == InventoryPage.Summary;
            _statsButton.IsPressed = page == InventoryPage.Stats;
            _miscButton.IsPressed = page == InventoryPage.Misc;
            return base.Render(extents, order);
        }
    }
}
