using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryCharacterPane : UiElement
    {
        const string SummaryButtonId = "Inventory.SummaryPage";
        const string StatsButtonId = "Inventory.StatsPage";
        const string MiscButtonId = "Inventory.MiscPage";

        static readonly HandlerSet Handlers = new HandlerSet(
            H<InventoryCharacterPane, ButtonPressEvent>((x,e) => x.OnButton(e.ButtonId))
        );

        void OnButton(string buttonId)
        {
            switch(buttonId)
            {
                case SummaryButtonId: _page = InventoryPage.Summary; break;
                case StatsButtonId: _page = InventoryPage.Stats; break;
                case MiscButtonId: _page = InventoryPage.Misc; break;
                default: return;
            }

            _summaryButton.IsPressed = _page == InventoryPage.Summary;
            _statsButton.IsPressed = _page == InventoryPage.Stats;
            _miscButton.IsPressed = _page == InventoryPage.Misc;
        }

        readonly Button _summaryButton;
        readonly Button _statsButton;
        readonly Button _miscButton;

        InventoryPage _page;

        public InventoryCharacterPane(PartyCharacterId activeCharacter) : base(Handlers)
        {
            _summaryButton = new Button(SummaryButtonId, "I") { DoubleFrame = true, IsPressed = _page == InventoryPage.Summary };
            _statsButton = new Button(StatsButtonId, "II") { DoubleFrame = true, IsPressed = _page == InventoryPage.Stats };
            _miscButton = new Button(MiscButtonId, "III") { DoubleFrame = true, IsPressed = _page == InventoryPage.Misc };
            var buttonStack = new HorizontalStack(
                new Padding(84,0),
                new FixedSize(16, 15, _summaryButton),
                new FixedSize(16, 15, _statsButton),
                new FixedSize(16, 15, _miscButton));

            var stack = new VerticalStack(
                new InventoryActivePageSelector(activeCharacter, () => _page),
                new Padding(0, 4),
                buttonStack
                );

            Children.Add(stack);
        }
    }
}