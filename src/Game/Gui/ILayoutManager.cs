using System.Collections.Generic;

namespace UAlbion.Game.Gui;

public interface ILayoutManager
{
    LayoutNode GetLayout();
    IDictionary<IUiElement, LayoutNode> LastSnapshot { get; }
    void RequestSnapshot();
}