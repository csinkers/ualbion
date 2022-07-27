namespace UAlbion.Game.Gui.Controls;

public class Greedy : UiElement, IGreedyUiElement
{
    public Greedy(IUiElement content) => AttachChild(content);
}