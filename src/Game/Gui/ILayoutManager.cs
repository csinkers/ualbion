using System.Collections.Generic;

namespace UAlbion.Game.Gui;

public interface ILayoutManager
{
    LayoutNode GetLayout();
    void RequestSnapshot();
    IDictionary<IUiElement, LayoutNode> LastSnapshot { get; }
    LayoutNode LastLayout { get; }
}