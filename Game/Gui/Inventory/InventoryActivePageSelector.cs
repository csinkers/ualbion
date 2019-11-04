using System;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using Veldrid;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryActivePageSelector : UiElement
    {
        readonly Func<InventoryPage> _getPage;
        readonly InventorySummaryPage _summary;
        readonly InventoryStatsPage _stats;
        readonly InventoryMiscPage _misc;

        public InventoryActivePageSelector(PartyCharacterId activeCharacter, Func<InventoryPage> getPage)
        {
            _getPage = getPage;
            _summary = new InventorySummaryPage(activeCharacter);
            _stats = new InventoryStatsPage();
            _misc = new InventoryMiscPage();
            Children.Add(_summary);
            Children.Add(_stats);
            Children.Add(_misc);
        }

        IUiElement GetActivePage() =>
            _getPage() switch
            {
                InventoryPage.Summary => (IUiElement)_summary,
                InventoryPage.Stats => _stats,
                InventoryPage.Misc => _misc, 
                InventoryPage x => throw new NotImplementedException($"Unhandled inventory page \"{x}\"")
            };
        public override int Render(Rectangle extents, int order, Action<IRenderable> addFunc) => GetActivePage().Render(extents, order, addFunc);
        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc) => GetActivePage().Select(uiPosition, extents, order, registerHitFunc);
    }
}