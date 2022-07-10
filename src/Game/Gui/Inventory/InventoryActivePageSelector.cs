using System;
using UAlbion.Core;
using UAlbion.Formats.Ids;

namespace UAlbion.Game.Gui.Inventory;

public class InventoryActivePageSelector : UiElement
{
    readonly PartyMemberId _activeCharacter;
    readonly Func<InventoryPage> _getPage;
    InventoryPage _lastPage = (InventoryPage)(object)-1;

    public InventoryActivePageSelector(PartyMemberId activeCharacter, Func<InventoryPage> getPage)
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
        RemoveAllChildren();

        IUiElement page = pageId switch
        {
            InventoryPage.Summary => new InventorySummaryPage(_activeCharacter),
            InventoryPage.Stats => new InventoryStatsPage(_activeCharacter),
            InventoryPage.Misc => new InventoryMiscPage(),
            { } x => throw new NotImplementedException($"Unhandled inventory page \"{x}\"")
        };

        AttachChild(page);
    }
    public override int Render(Rectangle extents, int order, LayoutNode parent)
    {
        ChangePage();
        return base.Render(extents, order, parent);
    }

    public override int Selection(Rectangle extents, int order, SelectionContext context)
    {
        ChangePage();
        return base.Selection(extents, order, context);
    }
}