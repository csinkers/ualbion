using System;
using System.Numerics;
using UAlbion.Formats.AssetIds;
using Veldrid;

namespace UAlbion.Game.Gui.Inventory
{
    public class InventoryActivePageSelector : UiElement
    {
        readonly PartyCharacterId _activeCharacter;
        readonly Func<InventoryPage> _getPage;
        InventoryPage _lastPage = (InventoryPage)(object)-1;

        public InventoryActivePageSelector(PartyCharacterId activeCharacter, Func<InventoryPage> getPage)
        {
            _activeCharacter = activeCharacter;
            _getPage = getPage;
        }

        void ChangePage()
        {
            var pageId = _getPage();
            if (pageId == _lastPage)
                return;

            _lastPage = pageId;
            foreach(var child in Children)
                child.Detach();
            Children.Clear();

            IUiElement page = pageId switch
            {
                InventoryPage.Summary => new InventorySummaryPage(_activeCharacter),
                InventoryPage.Stats => new InventoryStatsPage(_activeCharacter),
                InventoryPage.Misc => new InventoryMiscPage(_activeCharacter),
                InventoryPage x => throw new NotImplementedException($"Unhandled inventory page \"{x}\"")
            };

            AttachChild(page);
        }
        public override int Render(Rectangle extents, int order)
        {
            ChangePage();
            return base.Render(extents, order);
        }

        public override int Select(Vector2 uiPosition, Rectangle extents, int order, Action<int, object> registerHitFunc)
        {
            ChangePage();
            return base.Select(uiPosition, extents, order, registerHitFunc);
        }
    }
}
